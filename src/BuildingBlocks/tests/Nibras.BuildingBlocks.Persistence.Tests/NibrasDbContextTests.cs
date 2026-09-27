using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Persistence.Tests;

[Collection(PostgresDefinition.Name)]
public sealed class NibrasDbContextTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Guid NewTenant() => Guid.CreateVersion7();

    private static async Task SeedAsync(ServiceProvider provider, Guid tenant, params string[] names)
    {
        await using var scope = PostgresFixture.ScopeFor(provider, tenant);
        var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
        foreach (var name in names)
        {
            context.Students.Add(new Student(Guid.CreateVersion7(), name));
        }

        await context.SaveChangesAsync(Ct);
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-640")]
    public async Task A_context_leased_without_a_tenant_throws_on_its_first_query()
    {
        await using var provider = db.BuildProvider(poolSize: 2);
        await using var scope = PostgresFixture.ScopeFor(provider, tenant: null);
        var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();

        await Should.ThrowAsync<TenantNotSetException>(() => context.Students.AsNoTracking().ToListAsync(Ct));
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-640")]
    public async Task A_save_without_a_tenant_is_refused_before_it_reaches_the_database()
    {
        await using var provider = db.BuildProvider(poolSize: 2);
        await using var scope = PostgresFixture.ScopeFor(provider, tenant: null);
        var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
        context.Students.Add(new Student(Guid.CreateVersion7(), "Noura Al-Otaibi"));

        await Should.ThrowAsync<TenantNotSetException>(() => context.SaveChangesAsync(Ct));
    }

    [Fact]
    [Trait("TestCase", "TC-PERF-961")]
    public async Task One_pooled_context_serving_tenant_A_then_tenant_B_shows_the_second_only_tenant_B_rows()
    {
        var tenantA = NewTenant();
        var tenantB = NewTenant();
        await using var provider = db.BuildProvider(poolSize: 1);
        await SeedAsync(provider, tenantA, "Faisal Al-Dosari", "Joud Al-Harbi");
        await SeedAsync(provider, tenantB, "Tamim Suleiman");

        SchoolTestDbContext first;
        List<string> seenByA;
        await using (var scope = PostgresFixture.ScopeFor(provider, tenantA))
        {
            first = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            seenByA = await first.Students.AsNoTracking().OrderBy(s => s.FullName).Select(s => s.FullName).ToListAsync(Ct);
        }

        SchoolTestDbContext second;
        List<string> seenByB;
        await using (var scope = PostgresFixture.ScopeFor(provider, tenantB))
        {
            second = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            seenByB = await second.Students.AsNoTracking().Select(s => s.FullName).ToListAsync(Ct);
        }

        second.ShouldBeSameAs(first, "the pool of one must hand the same context to both scopes");
        seenByA.ShouldBe(["Faisal Al-Dosari", "Joud Al-Harbi"]);
        seenByB.ShouldBe(["Tamim Suleiman"]);
    }

    [Fact]
    [Trait("TestCase", "TC-PERF-961")]
    public async Task A_context_returned_to_the_pool_forgets_its_tenant()
    {
        await using var provider = db.BuildProvider(poolSize: 1);
        await SeedAsync(provider, NewTenant(), "Reem Al-Zahrani");

        await using var scope = PostgresFixture.ScopeFor(provider, tenant: null);
        var reused = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();

        await Should.ThrowAsync<TenantNotSetException>(() => reused.Students.AsNoTracking().CountAsync(Ct));
    }

    [Fact]
    [Trait("TestCase", "TC-PERF-962")]
    public async Task Every_tenant_owned_entity_carries_the_named_Tenant_and_SoftDelete_filters()
    {
        await using var provider = db.BuildProvider(poolSize: 1);
        await using var scope = PostgresFixture.ScopeFor(provider, NewTenant());
        var model = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>().Model;

        foreach (var entity in model.GetEntityTypes().Where(e => typeof(ITenantEntity).IsAssignableFrom(e.ClrType)))
        {
            entity.GetDeclaredQueryFilters().Select(f => f.Key).ShouldBe([QueryFilters.Tenant, QueryFilters.SoftDelete], ignoreOrder: true);
        }
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-008")]
    public async Task The_table_follows_the_naming_key_audit_and_concurrency_conventions()
    {
        await using var provider = db.BuildProvider(poolSize: 1);
        await using var scope = PostgresFixture.ScopeFor(provider, NewTenant());
        var entity = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>().Model.FindEntityType(typeof(Student))!;

        entity.GetTableName().ShouldBe("students");
        entity.GetProperties().Select(p => p.GetColumnName()).ShouldBe(
            ["tenant_id", "id", "created_at", "created_by", "deleted_at", "deleted_by", "full_name", "updated_at", "updated_by", "xmin"],
            ignoreOrder: true);
        entity.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()).ShouldBe(["tenant_id", "id"]);
        entity.FindProperty("Id")!.ValueGenerated.ShouldBe(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
        entity.FindProperty(AuditColumns.Xmin)!.IsConcurrencyToken.ShouldBeTrue();
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-008")]
    public async Task Audit_columns_come_from_the_injected_clock_and_a_removed_row_is_soft_deleted()
    {
        var tenant = NewTenant();
        var id = Guid.CreateVersion7();
        await using var provider = db.BuildProvider(poolSize: 2);
        var created = db.Clock.Now;
        await using (var scope = PostgresFixture.ScopeFor(provider, tenant))
        {
            var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            context.Students.Add(new Student(id, "Lina Barakat"));
            await context.SaveChangesAsync(Ct);
        }

        var removed = created.AddMinutes(5);
        await using (var scope = PostgresFixture.ScopeFor(provider, tenant))
        {
            db.Clock.Now = removed;
            var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            context.Students.Remove(await context.Students.SingleAsync(s => s.Id == id, Ct));
            await context.SaveChangesAsync(Ct);
        }

        await using (var scope = PostgresFixture.ScopeFor(provider, tenant))
        {
            var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            (await context.Students.AsNoTracking().AnyAsync(s => s.Id == id, Ct)).ShouldBeFalse("the SoftDelete filter hides it");

            var row = await context.Students
                .IgnoreQueryFilters([QueryFilters.SoftDelete]) // the recycle bin view: soft-deleted rows, still this tenant only
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.TenantId,
                    CreatedAt = EF.Property<DateTimeOffset>(s, AuditColumns.CreatedAt),
                    UpdatedAt = EF.Property<DateTimeOffset>(s, AuditColumns.UpdatedAt),
                    DeletedAt = EF.Property<DateTimeOffset?>(s, AuditColumns.DeletedAt),
                })
                .SingleAsync(Ct);
            row.TenantId.ShouldBe(tenant);
            row.CreatedAt.ShouldBe(created);
            row.UpdatedAt.ShouldBe(removed);
            row.DeletedAt.ShouldBe(removed);
        }
    }

    [Fact]
    [Trait("TestCase", "TC-PERF-968")]
    public async Task A_save_with_a_stale_xmin_is_refused()
    {
        var tenant = NewTenant();
        await using var provider = db.BuildProvider(poolSize: 4);
        var id = Guid.CreateVersion7();
        await using (var scope = PostgresFixture.ScopeFor(provider, tenant))
        {
            var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
            context.Students.Add(new Student(id, "Omar Haddad"));
            await context.SaveChangesAsync(Ct);
        }

        await using var first = PostgresFixture.ScopeFor(provider, tenant);
        await using var second = PostgresFixture.ScopeFor(provider, tenant);
        var one = first.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
        var two = second.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
        var readByOne = await one.Students.SingleAsync(s => s.Id == id, Ct);
        var readByTwo = await two.Students.SingleAsync(s => s.Id == id, Ct);

        readByOne.FullName = "Omar Haddad Al-Ali";
        await one.SaveChangesAsync(Ct);
        readByTwo.FullName = "O. Haddad";

        await Should.ThrowAsync<DbUpdateConcurrencyException>(() => two.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task An_entity_of_another_tenant_cannot_be_saved_in_this_scope()
    {
        var tenantA = NewTenant();
        var tenantB = NewTenant();
        await using var provider = db.BuildProvider(poolSize: 2);
        await SeedAsync(provider, tenantA, "Mariam Al-Shamsi");

        await using var scope = PostgresFixture.ScopeFor(provider, tenantB);
        var context = scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>();
        var foreign = new Student(Guid.CreateVersion7(), "Planted");
        context.Students.Add(foreign);
        context.Entry(foreign).Property(nameof(ITenantEntity.TenantId)).CurrentValue = tenantA;

        await Should.ThrowAsync<InvalidOperationException>(() => context.SaveChangesAsync(Ct));
    }
}
