using Microsoft.EntityFrameworkCore;
using Nibras.BuildingBlocks.Persistence;
using Nibras.BuildingBlocks.Testing.Isolation;
using Nibras.BuildingBlocks.Testing.Isolation.Canary;
using Xunit;

namespace Nibras.BuildingBlocks.Testing.Tests;

/// <summary>Starts one target's suite for a test class: its own database, host and seeded tenants.</summary>
public sealed class SuiteFixture<TTarget> : IAsyncLifetime
    where TTarget : IsolationTarget, new()
{
    public IsolationSuite Suite { get; private set; } = null!;

    public async ValueTask InitializeAsync() => Suite = await IsolationSuite.StartAsync(new TTarget(), TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() => await Suite.DisposeAsync();
}

/// <summary>The planted defect of the CAP-DATA-01 demo: every handler and the consumer read with the EF tenant filter bypassed.</summary>
public class FilterBypassedCanary : CanaryTarget
{
    protected override IQueryable<CanaryPupil> Pupils(CanaryDbContext db) => db.Pupils.IgnoreQueryFilters([QueryFilters.Tenant]);

    protected override IQueryable<CanarySection> Sections(CanaryDbContext db) => db.Sections.IgnoreQueryFilters([QueryFilters.Tenant]);
}

/// <summary>The filter bypassed and the row-level security policy never created: both barriers gone.</summary>
public sealed class UnprotectedCanary : FilterBypassedCanary
{
    protected override async Task CreateSchemaAsync(CanaryDbContext migrationContext, CancellationToken cancellationToken) =>
        await migrationContext.Database.EnsureCreatedAsync(cancellationToken);
}

/// <summary>A handler that sets the tenant for the session rather than the transaction, the mistake SET LOCAL exists to prevent.</summary>
public sealed class SessionLeakingCanary : CanaryTarget
{
    protected override IQueryable<CanaryPupil> Pupils(CanaryDbContext db)
    {
        db.Database.ExecuteSqlRaw("SELECT set_config('app.tenant_id', current_setting('app.tenant_id'), false)");
        return base.Pupils(db);
    }
}
