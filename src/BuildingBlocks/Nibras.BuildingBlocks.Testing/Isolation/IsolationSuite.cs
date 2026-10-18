using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Tenancy;
using Nibras.BuildingBlocks.Web;
using Nibras.Contracts.Shared;
using Npgsql;

namespace Nibras.BuildingBlocks.Testing.Isolation;

/// <summary>The verdict of one attack on one surface; <see cref="Detail"/> says what was observed.</summary>
public sealed record AttackOutcome(string Service, string Surface, SurfaceKind Kind, string Attack, bool Passed, string Detail);

/// <summary>The attack shapes of document 12 part 4.4 this suite runs. The others join with the slices that build their subject.</summary>
public static class IsolationAttacks
{
    /// <summary>Tenant B's identifiers in the path and the body, signed in as tenant A.</summary>
    public const string ForeignIdentifier = "foreign identifier";

    /// <summary>A request with no identifier, signed in as tenant A: the response must carry no row of tenant B.</summary>
    public const string ForeignRows = "foreign rows";

    /// <summary>An envelope of tenant B whose payload names tenant A's identifier.</summary>
    public const string ConsumerEnvelope = "consumer envelope";

    /// <summary>A tenant A transaction, the connection back in the pool, then tenant B, then a query in no transaction.</summary>
    public const string PoolSwap = "pool swap";
}

/// <summary>
/// The tenant-isolation attack suite of document 12 part 4.4 for one service: its database behind PgBouncer with one
/// server connection, its host, and tenants A and B seeded with the same shape of data. Every attack is signed in as
/// tenant A and carries tenant B's identifiers. Tenant B's rows are fingerprinted before and after each request, and
/// tenant A's around each consumer delivery, so a write across the boundary fails the attack even when the response
/// looks right.
/// </summary>
public sealed class IsolationSuite : IAsyncDisposable
{
    public const string PoolSwapSurface = "pooled connection";

    public static readonly TenantId TenantA = new(Guid.Parse("018f0000-0000-7000-8000-0000000000aa"));

    public static readonly TenantId TenantB = new(Guid.Parse("018f0000-0000-7000-8000-0000000000bb"));

    private static readonly DateTimeOffset DeliveredAt = new(2026, 9, 1, 8, 0, 0, TimeSpan.Zero);

    private readonly IsolationDatabase _database;
    private readonly WebApplication _host;
    private readonly HttpClient _client;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<Guid>> _rowsOfA;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<Guid>> _rowsOfB;
    private readonly IReadOnlySet<string> _tables;
    private readonly IReadOnlyList<string> _foreign;

    private IsolationSuite(
        IsolationTarget target,
        IsolationDatabase database,
        WebApplication host,
        IReadOnlyDictionary<string, IReadOnlyList<Guid>> rowsOfA,
        IReadOnlyDictionary<string, IReadOnlyList<Guid>> rowsOfB)
    {
        Target = target;
        _database = database;
        _host = host;
        _client = host.GetTestClient();
        _rowsOfA = rowsOfA;
        _rowsOfB = rowsOfB;
        _tables = target.TenantTables.Select(t => t.Table).ToHashSet(StringComparer.Ordinal);
        _foreign = [.. rowsOfB.Values.SelectMany(ids => ids).Append(TenantB.Value).Select(id => id.ToString("D"))];
        Surfaces = IsolationSurfaces.Of(((IEndpointRouteBuilder)host).DataSources, target.Consumers);
    }

    public IsolationTarget Target { get; }

    public IReadOnlyList<IsolationSurface> Surfaces { get; }

    /// <summary>Starts the database and the host of <paramref name="target"/> and seeds tenants A and B.</summary>
    public static async Task<IsolationSuite> StartAsync(IsolationTarget target, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        var database = await IsolationDatabase.StartAsync(target.Schema, cancellationToken).ConfigureAwait(false);
        WebApplication? host = null;
        try
        {
            await target.CreateSchemaAsync(database.MigrationConnectionString, cancellationToken).ConfigureAwait(false);
            host = target.BuildHost(database.ApplicationConnectionString);
            await host.StartAsync(cancellationToken).ConfigureAwait(false);
            TenantId[] tenants = [TenantA, TenantB];
            foreach (var tenant in tenants)
            {
                var scope = host.Services.CreateAsyncScope();
                await using (scope.ConfigureAwait(false))
                {
                    scope.ServiceProvider.GetRequiredService<TenantContext>().Set(tenant);
                    await target.SeedAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
                }
            }

            var rowsOfA = await RowsAsync(database, target, TenantA, cancellationToken).ConfigureAwait(false);
            var rowsOfB = await RowsAsync(database, target, TenantB, cancellationToken).ConfigureAwait(false);
            if (rowsOfB.Values.Sum(ids => ids.Count) == 0)
            {
                throw new InvalidOperationException($"{target.Service} seeded no row for tenant B, so no attack could leak one; seed every tenant the same way.");
            }

            return new IsolationSuite(target, database, host, rowsOfA, rowsOfB);
        }
        catch
        {
            if (host is not null)
            {
                await host.DisposeAsync().ConfigureAwait(false);
            }

            await database.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Attacks one surface by name, as the generated test for it does.</summary>
    public Task<AttackOutcome> AttackAsync(string surface, CancellationToken cancellationToken)
    {
        var found = Surfaces.FirstOrDefault(s => s.Name == surface)
            ?? throw new ArgumentException($"{Target.Service} has no surface '{surface}'. Regenerate its isolation tests.", nameof(surface));
        return found.Kind == SurfaceKind.Consumer ? ConsumerAsync(found, cancellationToken) : EndpointAsync(found, cancellationToken);
    }

    /// <summary>Every surface, then the pool swap, one after the other.</summary>
    public async Task<IReadOnlyList<AttackOutcome>> AttackAllAsync(CancellationToken cancellationToken)
    {
        var outcomes = new List<AttackOutcome>();
        foreach (var surface in Surfaces)
        {
            outcomes.Add(await AttackAsync(surface.Name, cancellationToken).ConfigureAwait(false));
        }

        outcomes.Add(await PoolSwapAsync(cancellationToken).ConfigureAwait(false));
        return outcomes;
    }

    /// <summary>
    /// The pool swap (reference architecture Section 14): tenant A uses the service, the server connection goes back to
    /// the pool of one, tenant B's transaction must read only tenant B's rows, and a query in no transaction must be
    /// refused, because a tenant setting that outlives its transaction would hand it the previous tenant's rows.
    /// </summary>
    public async Task<AttackOutcome> PoolSwapAsync(CancellationToken cancellationToken)
    {
        var surface = new IsolationSurface(SurfaceKind.Connection, PoolSwapSurface, "", null, null);
        var table = Target.TenantTables.FirstOrDefault(t => t.HasId && _rowsOfA[t.Table].Count > 0);
        if (table is null)
        {
            return Outcome(surface, IsolationAttacks.PoolSwap, false, "no tenant-owned table with an id column holds rows of tenant A");
        }

        // Tenant A goes through the service's own code first: its first list, as a request would.
        if (Surfaces.FirstOrDefault(s => s.Kind == SurfaceKind.Endpoint && HttpMethods.IsGet(s.Method) && s.IdentifierParameters.Count == 0) is { } list)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Path(list.Route!, _ => null).Path, UriKind.Relative));
            Target.Authenticate(request, TenantA);
            using var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if ((int)response.StatusCode >= 500)
            {
                return Outcome(surface, IsolationAttacks.PoolSwap, false, $"tenant A's {list.Name} answered {(int)response.StatusCode}");
            }
        }

        var seenByA = await ReadThroughServiceAsync(TenantA, table, cancellationToken).ConfigureAwait(false);
        var seenByB = await ReadThroughServiceAsync(TenantB, table, cancellationToken).ConfigureAwait(false);
        long? unbound;
        try
        {
            var connection = new NpgsqlConnection(_database.ApplicationConnectionString);
            await using (connection.ConfigureAwait(false))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                var command = new NpgsqlCommand("SELECT count(*) FROM " + table.Qualified, connection);
                await using (command.ConfigureAwait(false))
                {
                    unbound = (long?)await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (PostgresException)
        {
            unbound = null; // refused: app.tenant_id is unset outside a transaction, which is the design
        }

        var leakedToB = seenByB.Intersect(_rowsOfA[table.Table]).Count();
        var failure =
            !seenByA.ToHashSet().SetEquals(_rowsOfA[table.Table]) ? $"tenant A's own transaction read {seenByA.Count} rows of {table.Table}, not its {_rowsOfA[table.Table].Count}"
            : leakedToB > 0 ? $"tenant B's transaction read {leakedToB} rows of tenant A's {table.Table} after tenant A used the pooled connection"
            : !seenByB.ToHashSet().SetEquals(_rowsOfB[table.Table]) ? $"tenant B's transaction read {seenByB.Count} rows of {table.Table}, not its {_rowsOfB[table.Table].Count}"
            : unbound is { } count ? string.Create(CultureInfo.InvariantCulture, $"a query in no transaction read {count} rows of {table.Table}: a tenant setting outlived its transaction on the pooled connection")
            : null;
        return Outcome(surface, IsolationAttacks.PoolSwap, failure is null, failure ?? "tenant B read only its own rows and a query in no transaction was refused");
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _host.StopAsync().ConfigureAwait(false);
        await _host.DisposeAsync().ConfigureAwait(false);
        await _database.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<AttackOutcome> EndpointAsync(IsolationSurface surface, CancellationToken cancellationToken)
    {
        var sent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var (path, unresolved) = Path(surface.Route!, resource => ForeignRow(ResourceNames.TableFor(resource, Target, _tables)));
        if (unresolved is not null)
        {
            return Outcome(surface, IsolationAttacks.ForeignIdentifier, false, unresolved);
        }

        foreach (var id in path.Split('/').Where(s => Guid.TryParse(s, out _)))
        {
            sent.Add(id);
        }

        using var request = new HttpRequestMessage(new HttpMethod(surface.Method), new Uri(path, UriKind.Relative));
        Target.Authenticate(request, TenantA);
        if (HttpMethods.IsPost(surface.Method))
        {
            request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString("D"));
        }

        if (HttpMethods.IsPut(surface.Method) || HttpMethods.IsPatch(surface.Method) || HttpMethods.IsDelete(surface.Method))
        {
            request.Headers.TryAddWithoutValidation("If-Match", "\"0-00000000\""); // well formed, names no version
        }

        if (surface.BodyType is { } bodyType)
        {
            var body = RequestBodies.Sample(bodyType, property =>
            {
                var id = ResourceNames.ResourcesFor(property)
                    .Select(resource => ForeignRow(ResourceNames.TableFor(resource, Target, _tables)))
                    .FirstOrDefault(found => found is not null);
                if (id is { } value)
                {
                    sent.Add(value.ToString("D"));
                }

                return id;
            });
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        }

        var before = await FingerprintAsync(TenantB, cancellationToken).ConfigureAwait(false);
        using var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var after = await FingerprintAsync(TenantB, cancellationToken).ConfigureAwait(false);

        var status = (int)response.StatusCode;
        var attack = sent.Count > 0 ? IsolationAttacks.ForeignIdentifier : IsolationAttacks.ForeignRows;
        var leaked = _foreign.FirstOrDefault(id => !sent.Contains(id) && text.Contains(id, StringComparison.OrdinalIgnoreCase));
        var failure =
            status >= 500 ? Invariant($"answered {status}; a server error under attack proves nothing and may leak, so the handler must refuse cleanly")
            : before != after ? Invariant($"changed tenant B's rows and answered {status}")
            : leaked is not null ? Invariant($"answered {status} carrying tenant B's identifier {leaked}")
            : sent.Count > 0 && !Refused(status, text) ? Invariant($"answered {status} to tenant B's identifier; expected 404, or 403 with _TENANT_MISMATCH")
            : null;
        var passed = sent.Count > 0 ? Invariant($"refused tenant B's identifier with {status}") : Invariant($"answered {status} with no row of tenant B");
        return Outcome(surface, attack, failure is null, failure ?? passed);
    }

    private async Task<AttackOutcome> ConsumerAsync(IsolationSurface surface, CancellationToken cancellationToken)
    {
        var consumer = Target.Consumers.Single(c => IsolationSurface.ConsumerPrefix + c.Name == surface.Name);
        var table = ResourceNames.TableFor(consumer.Resource, Target, _tables);
        if (table is null || _rowsOfA[table].Count == 0)
        {
            return Outcome(surface, IsolationAttacks.ConsumerEnvelope, false, $"'{consumer.Resource}' names no tenant-owned table of {Target.Service} with rows; map it in ResourceTables.");
        }

        var foreign = _rowsOfA[table][0];
        var envelope = new MessageEnvelope(Guid.CreateVersion7(), Guid.CreateVersion7(), null, TenantB.Value, DeliveredAt, 1, foreign.ToString("D"));
        var before = await FingerprintAsync(TenantA, cancellationToken).ConfigureAwait(false);
        string handled;
        try
        {
            await consumer.Deliver(_host.Services, envelope, foreign, cancellationToken).ConfigureAwait(false);
            handled = "applied to nothing of tenant A";
        }
#pragma warning disable CA1031 // a consumer rejects a message by throwing; which exception it throws is not the verdict
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            handled = "rejected with " + ex.GetType().Name;
        }

        var after = await FingerprintAsync(TenantA, cancellationToken).ConfigureAwait(false);
        return before == after
            ? Outcome(surface, IsolationAttacks.ConsumerEnvelope, true, handled)
            : Outcome(surface, IsolationAttacks.ConsumerEnvelope, false, "an envelope of tenant B naming tenant A's identifier changed tenant A's rows");
    }

    /// <summary>The request path, with tenant B's identifier for every identifier parameter, named by the literal before it.</summary>
    private (string Path, string? Unresolved) Path(RoutePattern route, Func<string?, Guid?> identifierFor)
    {
        var path = new StringBuilder();
        string? resource = null;
        foreach (var segment in route.PathSegments)
        {
            path.Append('/');
            foreach (var part in segment.Parts)
            {
                switch (part)
                {
                    case RoutePatternLiteralPart literal:
                        path.Append(literal.Content);
                        resource = literal.Content;
                        break;
                    case RoutePatternSeparatorPart separator:
                        path.Append(separator.Content);
                        break;
                    case RoutePatternParameterPart parameter when IsolationSurface.IsIdentifier(parameter.Name):
                        if (identifierFor(resource) is not { } id)
                        {
                            return ("", $"'{{{parameter.Name}}}' follows '{resource}', which names no tenant-owned table of {Target.Service} with rows of tenant B; name the table after the resource or map it in ResourceTables.");
                        }

                        path.Append(id.ToString("D"));
                        break;
                    case RoutePatternParameterPart:
                        path.Append("isolation");
                        break;
                }
            }
        }

        return (path.ToString(), null);
    }

    private Guid? ForeignRow(string? table) =>
        table is not null && _rowsOfB.TryGetValue(table, out var ids) && ids.Count > 0 ? ids[0] : null;

    private static bool Refused(int status, string body)
    {
        if (status == StatusCodes.Status404NotFound)
        {
            return true;
        }

        if (status != StatusCodes.Status403Forbidden)
        {
            return false;
        }

        try
        {
            using var json = JsonDocument.Parse(body);
            return json.RootElement.TryGetProperty("code", out var code)
                && code.GetString() is { } value
                && value.EndsWith("_" + ErrorCatalog.TenantMismatch, StringComparison.Ordinal);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task<IReadOnlyList<Guid>> ReadThroughServiceAsync(TenantId tenant, TenantTable table, CancellationToken cancellationToken)
    {
        var scope = _host.Services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            scope.ServiceProvider.GetRequiredService<TenantContext>().Set(tenant);
            return await Target.ReadIdsAsync(scope.ServiceProvider, table, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>A digest of every row a tenant owns, read as the superuser so that row-level security cannot hide a change.</summary>
    private async Task<string> FingerprintAsync(TenantId tenant, CancellationToken cancellationToken)
    {
        var digest = new StringBuilder();
        var connection = new NpgsqlConnection(_database.SuperuserConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            foreach (var table in Target.TenantTables)
            {
                var order = table.HasId ? "t.id" : "t::text";
                var command = new NpgsqlCommand(
                    $"SELECT md5(coalesce(string_agg(t::text, '|' ORDER BY {order}), '')) FROM {table.Qualified} t WHERE t.tenant_id = @tenant",
                    connection);
                await using (command.ConfigureAwait(false))
                {
                    command.Parameters.AddWithValue("tenant", tenant.Value);
                    digest.Append(table.Table).Append('=').Append(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)).Append(';');
                }
            }
        }

        return digest.ToString();
    }

    private static async Task<IReadOnlyDictionary<string, IReadOnlyList<Guid>>> RowsAsync(
        IsolationDatabase database, IsolationTarget target, TenantId tenant, CancellationToken cancellationToken)
    {
        var rows = new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.Ordinal);
        var connection = new NpgsqlConnection(database.SuperuserConnectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            foreach (var table in target.TenantTables.Where(t => t.HasId))
            {
                var ids = new List<Guid>();
                var command = new NpgsqlCommand($"SELECT id FROM {table.Qualified} WHERE tenant_id = @tenant ORDER BY id", connection);
                await using (command.ConfigureAwait(false))
                {
                    command.Parameters.AddWithValue("tenant", tenant.Value);
                    var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    await using (reader.ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            ids.Add(reader.GetGuid(0));
                        }
                    }
                }

                rows[table.Table] = ids;
            }
        }

        return rows;
    }

    private AttackOutcome Outcome(IsolationSurface surface, string attack, bool passed, string detail) =>
        new(Target.Service, surface.Name, surface.Kind, attack, passed, detail);

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
