using System.Globalization;
using System.Text;
using System.Threading.Channels;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nibras.BuildingBlocks.Persistence;
using Nibras.BuildingBlocks.Tenancy;
using Nibras.BuildingBlocks.Testing.Isolation;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace Nibras.BuildingBlocks.Messaging.Tests;

/// <summary>A message as a test queue received it, copied out of the broker's buffers.</summary>
public sealed record Delivered(string RoutingKey, IReadOnlyBasicProperties Properties, byte[] Body)
{
    public string? Header(string name) =>
        Properties.Headers?.TryGetValue(name, out var value) == true
            ? value switch
            {
                byte[] bytes => Encoding.UTF8.GetString(bytes),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture),
            }
            : null;

    public string Text => Encoding.UTF8.GetString(Body);
}

/// <summary>
/// PostgreSQL 18.6 with the probe service's schema and outbox behind PgBouncer 1.25.2 (one server connection), and
/// RabbitMQ 4.3.6, the broker version document 19 pins. Each test starts its own hosts, and uses its own tenants and
/// queues, so a row one test leaves pending is never dispatched into another.
/// </summary>
public sealed class MessagingFixture : IAsyncLifetime
{
    public const string BrokerImage = "rabbitmq:4.3.6";

    private readonly IContainer _broker = new ContainerBuilder(BrokerImage)
        .WithEnvironment("RABBITMQ_DEFAULT_USER", "nibras")
        .WithEnvironment("RABBITMQ_DEFAULT_PASS", "nibras-test-only")
        .WithPortBinding(5672, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Server startup complete"))
        .Build();

    public IsolationDatabase Database { get; private set; } = null!;

    public Uri Broker { get; private set; } = null!;

    public IConnection Connection { get; private set; } = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        // Four server connections: the horizon test holds one transaction open while another commits.
        var database = IsolationDatabase.StartAsync("probe", serverConnections: 4, Ct);
        await _broker.StartAsync(Ct);
        Database = await database;
        Broker = new Uri($"amqp://nibras:nibras-test-only@{_broker.Hostname}:{_broker.GetMappedPublicPort(5672)}/");
        Connection = await new ConnectionFactory { Uri = Broker }.CreateConnectionAsync(Ct);

        // Tables owned by the migration role: the model, its policies, then the outbox and today's partitions.
        var services = new ServiceCollection();
        services.AddNibrasDbContext<ProbeDbContext>(Database.MigrationConnectionString, poolSize: 1);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
        await context.Database.EnsureCreatedAsync(Ct);
        await context.Database.ExecuteSqlRawAsync(RowLevelSecurity.PolicySqlFor(context), Ct);
        await context.Database.ExecuteSqlRawAsync(OutboxSchema.CreateSql("probe"), Ct);
        var today = DateOnly.FromDateTime(TimeProvider.System.GetUtcNow().UtcDateTime);
        for (var day = -1; day <= 2; day++)
        {
            await context.Database.ExecuteSqlRawAsync(OutboxSchema.PartitionSql("probe", today.AddDays(day)), Ct);
        }
    }

    /// <summary>
    /// A started host of the probe service with the messaging block, as a service's Program.cs wires it. By default it
    /// returns once its dispatcher leads, so a commit's notification cannot arrive before the leader listens.
    /// </summary>
    public async Task<IHost> StartHostAsync(Action<MessagingOptions>? configure = null, ICurrentUser? user = null, Uri? broker = null, bool waitForLeader = true)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = "Test", DisableDefaults = true });
        builder.Logging.ClearProviders();
        if (user is not null)
        {
            builder.Services.AddScoped(_ => user);
        }

        builder.Services.AddNibrasDbContext<ProbeDbContext>(Database.ApplicationConnectionString, poolSize: 4);
        builder.Services.AddNibrasMessaging<ProbeDbContext>(options =>
        {
            options.Service = "Probe";
            options.Broker = broker ?? Broker;
            options.ApplicationConnectionString = Database.ApplicationConnectionString;
            options.SessionConnectionString = Database.DirectApplicationConnectionString;
            options.StandbyRetry = TimeSpan.FromMilliseconds(500);
            configure?.Invoke(options);
        });
        var host = builder.Build();
        await host.StartAsync(Ct);
        if (waitForLeader)
        {
            var dispatcher = host.Services.GetRequiredService<OutboxDispatcher>();
            await UntilAsync(dispatcher, () => Task.FromResult(dispatcher.IsLeader));
        }

        return host;
    }

    /// <summary>A handler's unit of work: one state change and its events, committed or rolled back together.</summary>
    public static async Task CommitAsync(IHost host, Guid tenant, Action<IIntegrationEventPublisher, MessageContext> enqueue, bool commit = true)
    {
        await using var scope = host.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
        var db = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(Ct);
        db.Items.Add(new ProbeItem(Guid.CreateVersion7(), "probe"));
        enqueue(scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>(), scope.ServiceProvider.GetRequiredService<MessageContext>());
        await db.SaveChangesAsync(Ct);
        if (commit)
        {
            await transaction.CommitAsync(Ct);
        }
        else
        {
            await transaction.RollbackAsync(Ct);
        }
    }

    /// <summary>Waits, one dispatcher round at a time, until the condition holds.</summary>
    public static async Task UntilAsync(OutboxDispatcher dispatcher, Func<Task<bool>> condition, int seconds = 20)
    {
        var rounds = Channel.CreateUnbounded<bool>();
        void Signal() => rounds.Writer.TryWrite(true);
        dispatcher.RoundCompleted += Signal;
        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(Ct);
            deadline.CancelAfter(TimeSpan.FromSeconds(seconds));
            while (!await condition())
            {
                await rounds.Reader.ReadAsync(deadline.Token);
            }
        }
        catch (OperationCanceledException) when (!Ct.IsCancellationRequested)
        {
            throw new TimeoutException($"The condition did not hold within {seconds} s of dispatcher rounds.");
        }
        finally
        {
            dispatcher.RoundCompleted -= Signal;
        }
    }

    /// <summary>Waits for <paramref name="count"/> further rounds of the dispatcher.</summary>
    public static Task RoundsAsync(OutboxDispatcher dispatcher, int count)
    {
        var seen = 0;
        return UntilAsync(dispatcher, () => Task.FromResult(seen++ >= count));
    }

    /// <summary>One outbox row as the superuser sees it, without row-level security.</summary>
    public async Task<(DateTimeOffset? DispatchedAt, int Attempts, string? LastError)?> RowAsync(string bodyFragment)
    {
        await using var connection = new NpgsqlConnection(Database.SuperuserConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(
            "SELECT dispatched_at, dispatch_attempts, last_error FROM probe.outbox_messages WHERE convert_from(body, 'UTF8') LIKE @fragment", connection);
        command.Parameters.AddWithValue("fragment", "%" + bodyFragment + "%");
        await using var reader = await command.ExecuteReaderAsync(Ct);
        if (!await reader.ReadAsync(Ct))
        {
            return null;
        }

        return (reader.IsDBNull(0) ? null : reader.GetFieldValue<DateTimeOffset>(0), reader.GetInt32(1), reader.IsDBNull(2) ? null : reader.GetString(2));
    }

    public async ValueTask DisposeAsync()
    {
        if (Connection is not null)
        {
            await Connection.DisposeAsync();
        }

        await _broker.DisposeAsync();
        if (Database is not null)
        {
            await Database.DisposeAsync();
        }
    }
}

/// <summary>A test queue bound to the probe exchange, optionally consumed into an in-memory channel.</summary>
public sealed class QueueProbe : IAsyncDisposable
{
    private readonly IChannel _channel;
    private readonly Channel<Delivered> _deliveries = Channel.CreateUnbounded<Delivered>();

    private QueueProbe(IChannel channel, string queue)
    {
        _channel = channel;
        Queue = queue;
    }

    public string Queue { get; }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static async Task<QueueProbe> BindAsync(IConnection connection, string queue, string bindingKey, IDictionary<string, object?>? arguments = null, bool consume = true)
    {
        var channel = await connection.CreateChannelAsync(new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true), Ct);
        await MessagingTopology.DeclareAsync(channel, "Probe", Ct);
        await channel.QueueDeclareAsync(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments ?? new Dictionary<string, object?>(StringComparer.Ordinal) { ["x-queue-type"] = "quorum" },
            cancellationToken: Ct);
        await channel.QueuePurgeAsync(queue, Ct);
        if (bindingKey.Length > 0)
        {
            await channel.QueueBindAsync(queue, MessagingTopology.Exchange("Probe"), bindingKey, cancellationToken: Ct);
        }

        var probe = new QueueProbe(channel, queue);
        if (consume)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, delivery) =>
            {
                probe._deliveries.Writer.TryWrite(new Delivered(delivery.RoutingKey, delivery.BasicProperties, delivery.Body.ToArray()));
                return Task.CompletedTask;
            };
            await channel.BasicConsumeAsync(queue, autoAck: true, consumer, Ct);
        }

        return probe;
    }

    public async Task<Delivered> NextAsync(int seconds = 20) =>
        await _deliveries.Reader.ReadAsync(Ct).AsTask().WaitAsync(TimeSpan.FromSeconds(seconds), Ct);

    public bool TryNext(out Delivered? delivered) => _deliveries.Reader.TryRead(out delivered);

    public Task<uint> CountAsync() => _channel.MessageCountAsync(Queue, Ct);

    public Task<uint> PurgeAsync() => _channel.QueuePurgeAsync(Queue, Ct);

    /// <summary>
    /// Publishes straight into a length-limited queue until the broker refuses one. A quorum queue on RabbitMQ 4.3.6
    /// refuses only once it is over its limit, not when it reaches it, so "full" is the first negative confirm.
    /// </summary>
    public async Task FillAsync()
    {
        for (var i = 0; i < 10; i++)
        {
            try
            {
                await _channel.BasicPublishAsync(exchange: "", routingKey: Queue, mandatory: true, basicProperties: new BasicProperties(), body: Encoding.UTF8.GetBytes("filler"), Ct);
            }
            catch (PublishException)
            {
                return;
            }
        }

        throw new InvalidOperationException($"{Queue} accepted 10 messages without refusing one; is it length-limited with reject-publish?");
    }

    public async ValueTask DisposeAsync()
    {
        if (Queue.EndsWith(".unrouted", StringComparison.Ordinal))
        {
            await _channel.QueuePurgeAsync(Queue, CancellationToken.None);
        }
        else
        {
            await _channel.QueueDeleteAsync(Queue, cancellationToken: CancellationToken.None);
        }

        await _channel.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class MessagingDefinition : ICollectionFixture<MessagingFixture>
{
    public const string Name = "messaging";
}
