using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Persistence;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Nibras.BuildingBlocks.Messaging;

/// <summary>One undispatched row as the dispatcher reads it.</summary>
internal sealed record OutboxRow(
    Guid Id,
    string Exchange,
    string RoutingKey,
    string MessageType,
    int SchemaVersion,
    string? PartitionKey,
    Guid CorrelationId,
    Guid? CausationId,
    Guid? UserId,
    DateTimeOffset OccurredAt,
    string? Traceparent,
    string? Lane,
    string? Channel,
    byte[] Body);

/// <summary>
/// The relay of document 11 part 8.3. Every host of the service runs one; the leader is the host holding the
/// advisory lock <c>nibras.outbox.&lt;service&gt;</c> on a session connection straight to PostgreSQL, and standbys retry
/// every 5 seconds. The leader wakes on <c>LISTEN</c>, polls every second, works tenant by tenant under row-level
/// security, reads only rows below the oldest running transaction, publishes them in sequence order with publisher
/// confirms, and marks a row dispatched only once the broker confirmed it (REQ-MSG-009, REQ-MSG-016).
/// </summary>
public sealed partial class OutboxDispatcher : BackgroundService
{
    private const string OccurredAtFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffff'Z'";

    private readonly MessagingOptions _options;
    private readonly string _schema;
    private readonly IOutboxTenants _tenants;
    private readonly TimeProvider _clock;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly ConcurrentQueue<Guid> _woken = new();
    private readonly Dictionary<Guid, DateTimeOffset> _hot = [];
    private readonly HashSet<Guid> _known = [];
    private readonly ConcurrentDictionary<Guid, (long Pending, DateTimeOffset? Oldest)> _backlog = new();
    private readonly Counter<long> _dispatched;
    private readonly Counter<long> _failures;
    private readonly ActivitySource _activities;
    private DateTimeOffset _lastSweep = DateTimeOffset.MinValue;
    private int _cursor;

    internal OutboxDispatcher(MessagingOptions options, string schema, IOutboxTenants tenants, TimeProvider clock, NibrasMetrics? metrics, ILogger<OutboxDispatcher> logger)
    {
        _options = options;
        _schema = RowLevelSecurity.Identifier(schema);
        _tenants = tenants;
        _clock = clock;
        _logger = logger;
        _activities = ActivitySources.For(options.Service);
        metrics ??= new NibrasMetrics(options.Service);
        var service = new KeyValuePair<string, object?>("service", options.Service);
        _dispatched = metrics.Counter("nibras_outbox_dispatched_total", description: "Outbox rows the broker confirmed");
        _failures = metrics.Counter("nibras_outbox_publish_failures_total", description: "Negative confirms, timeouts and closed channels");
        metrics.Gauge("nibras_outbox_pending_messages", () => [new(_backlog.Values.Sum(b => b.Pending), service)], description: "Rows with dispatched_at null");
        metrics.Gauge("nibras_outbox_oldest_pending_seconds", () => [new(OldestPendingSeconds(), service)], "s", "Age of the oldest undispatched row; the OutboxLag signal");
    }

    /// <summary>True while this host holds the lock and publishes.</summary>
    public bool IsLeader { get; private set; }

    /// <summary>Raised after every dispatch round of the leader, and when an attempt to lead ends; tests wait on it.</summary>
    internal event Action? RoundCompleted;

    private string Table => _schema + "." + OutboxModel.Table;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await LeadAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
#pragma warning disable CA1031 // the relay must outlive any failure of the database or the broker; it steps down and tries again
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LeadershipLost(_logger, _options.Service, ex);
            }
            finally
            {
                IsLeader = false;
                RoundCompleted?.Invoke();
            }

            try
            {
                await Task.Delay(_options.StandbyRetry, _clock, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task LeadAsync(CancellationToken cancellationToken)
    {
        var session = new NpgsqlConnection(_options.SessionConnectionString);
        await using (session.ConfigureAwait(false))
        {
            await session.OpenAsync(cancellationToken).ConfigureAwait(false);
            var locked = await ScalarAsync<bool>(session, null, "SELECT pg_try_advisory_lock(hashtextextended(@key, 0))", cancellationToken, ("key", "nibras.outbox." + MessagingTopology.Segment(_options.Service)))
                .ConfigureAwait(false);
            if (!locked)
            {
                return; // a standby: the leader holds the lock
            }

            session.Notification += (_, e) =>
            {
                if (Guid.TryParse(e.Payload, out var tenant))
                {
                    _woken.Enqueue(tenant);
                }
            };
            await NonQueryAsync(session, null, "LISTEN " + OutboxSchema.NotifyChannel, cancellationToken).ConfigureAwait(false);

            var factory = new ConnectionFactory
            {
                Uri = _options.Broker!,
                ClientProvidedName = "nibras-" + MessagingTopology.Segment(_options.Service) + "-outbox",
                AutomaticRecoveryEnabled = false, // a broken connection ends the leadership; the next attempt starts clean
                TopologyRecoveryEnabled = false,
            };
            var connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using (connection.ConfigureAwait(false))
            {
                var channel = await connection.CreateChannelAsync(new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true), cancellationToken)
                    .ConfigureAwait(false);
                await using (channel.ConfigureAwait(false))
                {
                    await MessagingTopology.DeclareAsync(channel, _options.Service, cancellationToken).ConfigureAwait(false);
                    IsLeader = true;
                    BecameLeader(_logger, _options.Service);
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var more = await RoundAsync(channel, cancellationToken).ConfigureAwait(false);
                        RoundCompleted?.Invoke();
                        if (!more)
                        {
                            await session.WaitAsync(_options.PollInterval, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>One round: the hot tenants, or every known tenant on a sweep, round-robin, within the round's budget. True when more is waiting.</summary>
    private async Task<bool> RoundAsync(IChannel channel, CancellationToken cancellationToken)
    {
        var now = _clock.GetUtcNow();
        while (_woken.TryDequeue(out var woken))
        {
            _hot[woken] = now;
            _known.Add(woken);
        }

        IEnumerable<Guid> candidates;
        if (now - _lastSweep >= _options.SweepInterval)
        {
            _known.UnionWith(await _tenants.ListAsync(cancellationToken).ConfigureAwait(false));
            _lastSweep = now;
            candidates = _known;
        }
        else
        {
            candidates = _hot.Where(t => now - t.Value <= _options.HotWindow).Select(t => t.Key);
        }

        var tenants = candidates.Order().ToList();
        var budget = _options.MaxPerRound;
        var more = false;
        for (var i = 0; i < tenants.Count; i++)
        {
            if (budget == 0)
            {
                more = true;
                break;
            }

            var tenant = tenants[(i + _cursor) % tenants.Count];
            var limit = Math.Min(_options.MaxPerTenant, budget);
            var (read, pending) = await DispatchAsync(channel, tenant, limit, cancellationToken).ConfigureAwait(false);
            budget -= read;
            more |= read == limit;
            if (pending > 0)
            {
                _hot[tenant] = now; // a tenant with work left stays hot
            }
        }

        _cursor++;
        foreach (var cold in _hot.Where(t => now - t.Value > _options.HotWindow).Select(t => t.Key).ToList())
        {
            _hot.Remove(cold);
        }

        return more;
    }

    /// <summary>Reads, publishes and marks one tenant's batch. Returns the rows read and the rows still pending before the publish.</summary>
    private async Task<(int Read, long Pending)> DispatchAsync(IChannel channel, Guid tenant, int limit, CancellationToken cancellationToken)
    {
        var rows = new List<OutboxRow>();
        long pending;
        var connection = new NpgsqlConnection(_options.ApplicationConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var read = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using (read.ConfigureAwait(false))
            {
                await SetTenantAsync(connection, read, tenant, cancellationToken).ConfigureAwait(false);
                var backlog = new NpgsqlCommand($"SELECT count(*), min(created_at) FROM {Table} WHERE dispatched_at IS NULL", connection, read);
                await using (backlog.ConfigureAwait(false))
                {
                    var reader = await backlog.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    await using (reader.ConfigureAwait(false))
                    {
                        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                        pending = reader.GetInt64(0);
                        _backlog[tenant] = (pending, reader.IsDBNull(1) ? null : reader.GetFieldValue<DateTimeOffset>(1));
                    }
                }

                var select = new NpgsqlCommand(
                    $"""
                    SELECT id, exchange, routing_key, message_type, schema_version, partition_key, correlation_id, causation_id,
                           user_id, occurred_at, traceparent, lane, channel, body
                    FROM {Table}
                    WHERE dispatched_at IS NULL AND transaction_id < pg_snapshot_xmin(pg_current_snapshot())
                    ORDER BY sequence
                    LIMIT @limit
                    """,
                    connection,
                    read);
                await using (select.ConfigureAwait(false))
                {
                    select.Parameters.AddWithValue("limit", limit);
                    var reader = await select.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    await using (reader.ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            rows.Add(new OutboxRow(
                                reader.GetGuid(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetInt32(4),
                                reader.IsDBNull(5) ? null : reader.GetString(5),
                                reader.GetGuid(6),
                                reader.IsDBNull(7) ? null : reader.GetGuid(7),
                                reader.IsDBNull(8) ? null : reader.GetGuid(8),
                                reader.GetFieldValue<DateTimeOffset>(9),
                                reader.IsDBNull(10) ? null : reader.GetString(10),
                                reader.IsDBNull(11) ? null : reader.GetString(11),
                                reader.IsDBNull(12) ? null : reader.GetString(12),
                                reader.GetFieldValue<byte[]>(13)));
                        }
                    }
                }

                await read.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (rows.Count == 0)
            {
                return (0, pending);
            }

            var outcomes = await PublishAsync(channel, tenant, rows, cancellationToken).ConfigureAwait(false);
            var (dispatched, failed) = Decide(rows, outcomes);
            var mark = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using (mark.ConfigureAwait(false))
            {
                await SetTenantAsync(connection, mark, tenant, cancellationToken).ConfigureAwait(false);
                if (dispatched.Count > 0)
                {
                    await NonQueryAsync(connection, mark, $"UPDATE {Table} SET dispatched_at = @now WHERE id = ANY(@ids)", cancellationToken, ("now", _clock.GetUtcNow()), ("ids", dispatched.Select(r => r.Id).ToArray()))
                        .ConfigureAwait(false);
                }

                if (failed.Count > 0)
                {
                    await NonQueryAsync(
                        connection,
                        mark,
                        $"UPDATE {Table} AS o SET dispatch_attempts = o.dispatch_attempts + 1, last_error = f.error FROM unnest(@ids, @errors) AS f(id, error) WHERE o.id = f.id",
                        cancellationToken,
                        ("ids", failed.Select(f => f.Row.Id).ToArray()),
                        ("errors", failed.Select(f => f.Error).ToArray())).ConfigureAwait(false);
                }

                await mark.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            foreach (var exchange in dispatched.GroupBy(r => r.Exchange))
            {
                _dispatched.Add(exchange.Count(), new("service", _options.Service), new("exchange", exchange.Key));
            }

            foreach (var reason in failed.GroupBy(f => f.Reason))
            {
                _failures.Add(reason.Count(), new("service", _options.Service), new("reason", reason.Key));
            }

            if (outcomes.Any(o => o.Reason == "timeout"))
            {
                // Confirms still outstanding on this channel would be misread later; step down and start a clean channel.
                throw new TimeoutException($"The broker confirmed none of {outcomes.Count(o => o.Reason == "timeout")} publishes within {_options.ConfirmTimeout}.");
            }

            return (rows.Count, pending);
        }
    }

    /// <summary>
    /// Which rows to mark dispatched. A row that failed blocks the later rows of its partition key in this batch, even
    /// when the broker confirmed them, so they are published again after it and per-key order is kept (document 11 part 6.2).
    /// </summary>
    internal static (List<OutboxRow> Dispatched, List<(OutboxRow Row, string Error, string Reason)> Failed) Decide(
        IReadOnlyList<OutboxRow> rows, IReadOnlyList<(string? Error, string? Reason)> outcomes)
    {
        var dispatched = new List<OutboxRow>();
        var failed = new List<(OutboxRow, string, string)>();
        var blocked = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var (error, reason) = outcomes[i];
            if (error is not null)
            {
                failed.Add((row, error, reason!));
                if (row.PartitionKey is { } key)
                {
                    blocked.Add(key);
                }
            }
            else if (row.PartitionKey is null || !blocked.Contains(row.PartitionKey))
            {
                dispatched.Add(row);
            }
        }

        return (dispatched, failed);
    }

    private async Task<IReadOnlyList<(string? Error, string? Reason)>> PublishAsync(IChannel channel, Guid tenant, List<OutboxRow> rows, CancellationToken cancellationToken)
    {
        // Pipelined: every publish is sent in sequence order, then the confirms are awaited together.
        var publishes = new List<Task>(rows.Count);
        foreach (var row in rows)
        {
            ActivityContext.TryParse(row.Traceparent, null, out var parent);
            using var activity = _activities.StartActivity("publish " + row.Exchange, ActivityKind.Producer, parent);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination.name", row.Exchange);
            activity?.SetTag("messaging.rabbitmq.destination.routing_key", row.RoutingKey);
            activity?.SetTag("messaging.message.id", row.Id.ToString("D"));
            activity?.SetTag("messaging.message.conversation_id", row.CorrelationId.ToString("D"));
            publishes.Add(channel.BasicPublishAsync(row.Exchange, row.RoutingKey, mandatory: false, Properties(row, tenant, activity), row.Body, cancellationToken).AsTask());
        }

        try
        {
            await Task.WhenAll(publishes).WaitAsync(_options.ConfirmTimeout, _clock, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // the publishes not yet confirmed are reported below
        }
#pragma warning disable CA1031 // each publish's own fault is read from its task below
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
#pragma warning restore CA1031
        {
            // at least one publish failed; which ones is read from the tasks
        }

        return [.. publishes.Select(Outcome)];
    }

    private static (string? Error, string? Reason) Outcome(Task publish) =>
        publish.Status switch
        {
            TaskStatus.RanToCompletion => (null, null),
            TaskStatus.Faulted => publish.Exception!.InnerException switch
            {
                PublishException { IsReturn: false } nack => ("negative confirm: " + nack.Message, "nack"),
                AlreadyClosedException or OperationInterruptedException => ("channel closed: " + publish.Exception.InnerException.Message, "closed"),
                var other => (other?.GetType().Name + ": " + other?.Message, "error"),
            },
            _ => ("no confirm within the timeout", "timeout"),
        };

    /// <summary>The envelope on the wire (document 11 part 2.1): AMQP properties for what the broker reads, headers for the rest.</summary>
    internal static BasicProperties Properties(OutboxRow row, Guid tenant, Activity? publish)
    {
        var headers = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["x-nibras-tenant-id"] = tenant.ToString("D"),
            ["x-nibras-occurred-at"] = row.OccurredAt.UtcDateTime.ToString(OccurredAtFormat, CultureInfo.InvariantCulture),
            ["x-nibras-schema-version"] = row.SchemaVersion,
        };
        Optional(headers, "x-nibras-causation-id", row.CausationId?.ToString("D"));
        Optional(headers, "x-nibras-user-id", row.UserId?.ToString("D"));
        Optional(headers, "x-partition-key", row.PartitionKey);
        Optional(headers, "traceparent", publish?.Id ?? row.Traceparent);
        Optional(headers, "tracestate", publish?.TraceStateString);
        Optional(headers, "x-nibras-lane", row.Lane);
        Optional(headers, "x-nibras-channel", row.Channel);
        return new BasicProperties
        {
            MessageId = row.Id.ToString("D"),
            CorrelationId = row.CorrelationId.ToString("D"),
            Type = row.MessageType,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(row.OccurredAt.ToUnixTimeSeconds()),
            Headers = headers,
        };
    }

    private static void Optional(Dictionary<string, object?> headers, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            headers[name] = value;
        }
    }

    private double OldestPendingSeconds()
    {
        var oldest = _backlog.Values.Where(b => b.Pending > 0 && b.Oldest is not null).Select(b => b.Oldest!.Value).DefaultIfEmpty(_clock.GetUtcNow()).Min();
        return Math.Max(0, (_clock.GetUtcNow() - oldest).TotalSeconds);
    }

    private static Task SetTenantAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid tenant, CancellationToken cancellationToken) =>
        NonQueryAsync(connection, transaction, $"SELECT set_config('{RowLevelSecurity.TenantSetting}', @tenant, true)", cancellationToken, ("tenant", tenant.ToString("D")));

    private static async Task NonQueryAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, CancellationToken cancellationToken, params (string Name, object Value)[] parameters)
    {
        var command = new NpgsqlCommand(sql, connection, transaction);
        await using (command.ConfigureAwait(false))
        {
            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value);
            }

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<T> ScalarAsync<T>(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, CancellationToken cancellationToken, params (string Name, object Value)[] parameters)
    {
        var command = new NpgsqlCommand(sql, connection, transaction);
        await using (command.ConfigureAwait(false))
        {
            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value);
            }

            return (T)(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false))!;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "The {Service} outbox dispatcher took the lead.")]
    private static partial void BecameLeader(ILogger logger, string service);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The {Service} outbox dispatcher stepped down and will try again.")]
    private static partial void LeadershipLost(ILogger logger, string service, Exception exception);
}
