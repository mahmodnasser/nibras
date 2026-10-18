using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Persistence;
using Nibras.BuildingBlocks.Tenancy;
using Nibras.Contracts.Shared;

namespace Nibras.BuildingBlocks.Testing.Isolation;

/// <summary>A tenant-owned table of a service: where the suite seeds, reads and fingerprints a tenant's rows.</summary>
/// <param name="Schema">The service's schema.</param>
/// <param name="Table">The table's name.</param>
/// <param name="HasId">True when the table has an <c>id</c> column, so its identifiers can be swapped into a request.</param>
public sealed record TenantTable(string Schema, string Table, bool HasId)
{
    public string Qualified => $"\"{Schema}\".\"{Table}\"";
}

/// <summary>
/// One consumer of a service, delivered as its consumer host delivers a message (document 12 part 4.4, consumer
/// swap). <see cref="Deliver"/> receives the host's root services, the envelope and the identifier the payload
/// carries, and sets the scope's tenant from the envelope, never from the payload. The inbox and the
/// dead-letter path join with SL-MSG-002.
/// </summary>
/// <param name="Name">The routing key consumed, for example <c>academics.section.closed.v1</c>.</param>
/// <param name="Resource">The resource the payload's identifier names, as its route segment would, for example <c>sections</c>.</param>
/// <param name="Deliver">Delivers one message; throwing is how a consumer rejects it.</param>
public sealed record IsolationConsumer(
    string Name,
    string Resource,
    Func<IServiceProvider, MessageEnvelope, Guid, CancellationToken, Task> Deliver);

/// <summary>
/// One service as the tenant-isolation suite attacks it (document 12 part 4.4): how its host starts over a database
/// the suite prepared, how one tenant's data is seeded, and which consumers it runs. Every service has one under
/// <c>tests/TenantIsolation.Tests/Targets/</c>, derived from <see cref="IsolationTarget{TContext}"/>.
/// </summary>
public abstract class IsolationTarget
{
    /// <summary>The header a host reads the tenant from until the identity slices issue tokens (document 22 §1.5).</summary>
    public const string TenantHeader = "X-Nibras-Tenant-Id";

    private protected IsolationTarget()
    {
    }

    /// <summary>The Appendix L service name, for example <c>Attendance</c>.</summary>
    public abstract string Service { get; }

    /// <summary>The service's consumers.</summary>
    public virtual IReadOnlyList<IsolationConsumer> Consumers => [];

    /// <summary>The table of each resource whose route segment, in snake case, is not its table's name.</summary>
    public virtual IReadOnlyDictionary<string, string> ResourceTables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The schema of the service, which also names its database and roles (<see cref="DatabaseBootstrap"/>).</summary>
    internal abstract string Schema { get; }

    /// <summary>The tenant-owned tables of the service's model, ordered by name.</summary>
    internal abstract IReadOnlyList<TenantTable> TenantTables { get; }

    /// <summary>The service's host on the test server, over the application role's connection string. Not started.</summary>
    public abstract WebApplication BuildHost(string connectionString);

    /// <summary>Seeds the scope's tenant with the shape of data every tenant gets, so tenant B mirrors tenant A.</summary>
    public abstract Task SeedAsync(IServiceProvider scope, CancellationToken cancellationToken);

    /// <summary>Signs a request in as a user of <paramref name="tenant"/>. Until the identity slices, the tenant header.</summary>
    public virtual void Authenticate(HttpRequestMessage request, TenantId tenant)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Headers.Add(TenantHeader, tenant.ToString());
    }

    internal abstract Task CreateSchemaAsync(string migrationConnectionString, CancellationToken cancellationToken);

    /// <summary>The ids of one table as the scope's tenant reads them in its own transaction, by raw SQL, so only row-level security binds.</summary>
    internal abstract Task<IReadOnlyList<Guid>> ReadIdsAsync(IServiceProvider scope, TenantTable table, CancellationToken cancellationToken);
}

/// <summary>An <see cref="IsolationTarget"/> whose data lives in <typeparamref name="TContext"/>.</summary>
public abstract class IsolationTarget<TContext> : IsolationTarget
    where TContext : NibrasDbContext
{
    private const string UnopenedConnection = "Host=127.0.0.1;Port=1;Database=unopened;Username=unopened;Password=unopened";

    private readonly Lazy<(string Schema, IReadOnlyList<TenantTable> Tables)> _model;

    protected IsolationTarget() => _model = new(ReadModel);

    internal override string Schema => _model.Value.Schema;

    internal override IReadOnlyList<TenantTable> TenantTables => _model.Value.Tables;

    internal override async Task CreateSchemaAsync(string migrationConnectionString, CancellationToken cancellationToken)
    {
        var provider = Provider(migrationConnectionString);
        await using (provider.ConfigureAwait(false))
        {
            var scope = provider.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                await CreateSchemaAsync(scope.ServiceProvider.GetRequiredService<TContext>(), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    internal override async Task<IReadOnlyList<Guid>> ReadIdsAsync(IServiceProvider scope, TenantTable table, CancellationToken cancellationToken)
    {
        var context = scope.GetRequiredService<TContext>();
#pragma warning disable EF1003 // the table name comes from the service's own EF model, quoted; nothing from a request reaches it
        return await context.InTenantTransactionAsync(
            async ct => (IReadOnlyList<Guid>)await context.Database.SqlQueryRaw<Guid>("SELECT id AS \"Value\" FROM " + table.Qualified).ToListAsync(ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
#pragma warning restore EF1003
    }

    /// <summary>
    /// Creates the tables as the migration role, which owns them. The default is the model and its row-level security
    /// policy; the service's migration bundle replaces it when SL-DATA-005 builds the bundle.
    /// </summary>
    protected virtual async Task CreateSchemaAsync(TContext migrationContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(migrationContext);
        await migrationContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        await migrationContext.Database.ExecuteSqlRawAsync(RowLevelSecurity.PolicySqlFor(migrationContext), cancellationToken).ConfigureAwait(false);
    }

    private static ServiceProvider Provider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddNibrasDbContext<TContext>(connectionString, poolSize: 1);
        return services.BuildServiceProvider();
    }

    private static (string, IReadOnlyList<TenantTable>) ReadModel()
    {
        using var provider = Provider(UnopenedConnection);
        using var scope = provider.CreateScope();
        var model = scope.ServiceProvider.GetRequiredService<TContext>().Model;
        var schema = model.GetDefaultSchema()
            ?? throw new InvalidOperationException($"{typeof(TContext).Name} has no default schema; NibrasDbContext always sets one.");
        var tables = model.GetEntityTypes()
            .Where(e => typeof(ITenantEntity).IsAssignableFrom(e.ClrType) && e.GetTableName() is not null)
            .Select(e => new TenantTable(e.GetSchema() ?? schema, e.GetTableName()!, HasIdColumn(e)))
            .OrderBy(t => t.Table, StringComparer.Ordinal)
            .ToList();
        return (schema, tables);
    }

    private static bool HasIdColumn(IEntityType entity) =>
        entity.FindProperty("Id") is { } id && id.ClrType == typeof(Guid) && id.GetColumnName() == "id";
}
