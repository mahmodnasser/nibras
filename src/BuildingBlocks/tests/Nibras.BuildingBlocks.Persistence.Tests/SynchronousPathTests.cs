using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Persistence.Tests;

/// <summary>
/// The synchronous twins of the paths the other suites prove asynchronously. A caller that uses <c>SaveChanges</c>,
/// <c>BeginTransaction</c> or a synchronous scope gets the same tenant, audit, soft-delete and pooling behaviour.
/// </summary>
[Collection(PostgresDefinition.Name)]
public sealed class SynchronousPathTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    [Trait("TestCase", "TC-DATA-008")]
    public void A_synchronous_save_sets_the_tenant_and_the_audit_columns_and_a_synchronous_remove_soft_deletes()
    {
        var tenant = Guid.CreateVersion7();
        using var provider = db.BuildProvider(poolSize: 2);
        var id = Guid.CreateVersion7();
        using (var scope = PostgresFixture.ScopeFor(provider, tenant))
        {
            var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            context.Students.Add(new Student(id, "Rakan Al-Shammari"));
            context.SaveChanges();
            context.Students.Remove(context.Students.Single(s => s.Id == id));
            context.SaveChanges();
        }

        using var check = PostgresFixture.ScopeFor(provider, tenant);
        var row = check.ServiceProvider.GetRequiredService<SchoolTestDbContext>().Students
            .IgnoreQueryFilters([QueryFilters.SoftDelete])
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.TenantId,
                CreatedAt = EF.Property<DateTimeOffset>(s, AuditColumns.CreatedAt),
                DeletedAt = EF.Property<DateTimeOffset?>(s, AuditColumns.DeletedAt),
            })
            .Single();
        row.TenantId.ShouldBe(tenant);
        row.CreatedAt.ShouldBe(db.Clock.Now);
        row.DeletedAt.ShouldBe(db.Clock.Now);
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-643")]
    public void A_synchronous_transaction_carries_the_tenant_as_its_first_statement()
    {
        var tenant = Guid.CreateVersion7();
        using var provider = db.BuildProvider(poolSize: 2);
        using var scope = PostgresFixture.ScopeFor(provider, tenant);
        var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();

        using var transaction = context.Database.BeginTransaction();
        var setting = context.Database
            .SqlQueryRaw<string>($"SELECT current_setting('{RowLevelSecurity.TenantSetting}', true) AS \"Value\"")
            .Single();

        setting.ShouldBe(tenant.ToString("D"));
    }

    [Fact]
    [Trait("TestCase", "TC-PERF-961")]
    public void A_context_returned_to_the_pool_by_a_synchronous_scope_has_lost_its_tenant()
    {
        using var provider = db.BuildProvider(poolSize: 1);
        SchoolTestDbContext first;
        using (var scope = PostgresFixture.ScopeFor(provider, Guid.CreateVersion7()))
        {
            first = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            _ = first.Students.AsNoTracking().Count();
        }

        using var next = PostgresFixture.ScopeFor(provider, tenant: null);
        var second = next.ServiceProvider.GetRequiredService<SchoolTestDbContext>();

        second.ShouldBeSameAs(first); // the pool of one handed the same instance back
        Should.Throw<TenantNotSetException>(() => second.Students.AsNoTracking().Count());
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-640")]
    public async Task A_tenant_transaction_without_a_tenant_is_refused_before_it_opens()
    {
        await using var provider = db.BuildProvider(poolSize: 1);
        await using var scope = PostgresFixture.ScopeFor(provider, tenant: null);
        var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();

        await Should.ThrowAsync<TenantNotSetException>(() => context.InTenantTransactionAsync(_ => Task.FromResult(1), Ct));
        context.Database.CurrentTransaction.ShouldBeNull();
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-953")]
    public void A_migration_adds_the_same_policy_the_model_generates_for_its_table()
    {
        var migration = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");

        migration.EnableTenantRowLevelSecurity("students", "school_test");

        migration.Operations.ShouldHaveSingleItem().ShouldBeOfType<SqlOperation>().Sql
            .ShouldBe(RowLevelSecurity.PolicySql("school_test", "students"));
        Should.Throw<ArgumentNullException>(() => RowLevelSecurity.EnableTenantRowLevelSecurity(null!, "students", "school_test"));
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-640")]
    public void The_tenant_exception_keeps_its_cause()
    {
        var cause = new InvalidOperationException("lease");

        var exception = new TenantNotSetException("No tenant.", cause);

        exception.InnerException.ShouldBeSameAs(cause);
        exception.Message.ShouldBe("No tenant.");
    }
}
