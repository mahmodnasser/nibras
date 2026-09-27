using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Persistence.Tests;

/// <summary>Row-level security as the second barrier, through PgBouncer in transaction mode (document 10, part 2.5).</summary>
[Collection(RowLevelSecurityDefinition.Name)]
public sealed class TenantIsolationTests(RowLevelSecurityFixture rls)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<Guid> SeedAsync(ServiceProvider provider, int count)
    {
        var tenant = Guid.CreateVersion7();
        await using var scope = PostgresFixture.ScopeFor(provider, tenant);
        var context = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
        for (var i = 0; i < count; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord(Guid.CreateVersion7(), "present"));
        }

        await context.SaveChangesAsync(Ct); // EF opens the transaction; the interceptor sets app.tenant_id in it
        return tenant;
    }

    private static async Task<int> CountAsync(ServiceProvider provider, Guid tenant, bool bypassEfTenantFilter = false)
    {
        await using var scope = PostgresFixture.ScopeFor(provider, tenant);
        var context = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
        return await context.InTenantTransactionAsync(
            ct => bypassEfTenantFilter
                ? context.AttendanceRecords.IgnoreQueryFilters([QueryFilters.Tenant]).AsNoTracking().CountAsync(ct) // the planted defect
                : context.AttendanceRecords.AsNoTracking().CountAsync(ct),
            Ct);
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-643")]
    public async Task A_pooled_connection_cannot_read_the_previous_tenant_rows()
    {
        await using var provider = RowLevelSecurityFixture.BuildProvider(rls.AppConnectionString, poolSize: 1);
        var tenantA = await SeedAsync(provider, 25);
        var tenantB = Guid.CreateVersion7();

        var readByA = await CountAsync(provider, tenantA);
        var readByB = await CountAsync(provider, tenantB);

        readByA.ShouldBe(25);
        readByB.ShouldBe(0);

        // A query that opens no transaction has no app.tenant_id: PostgreSQL refuses it rather than returning rows.
        await using (var connection = new NpgsqlConnection(rls.AppConnectionString))
        {
            await connection.OpenAsync(Ct);
            await using var command = new NpgsqlCommand("SELECT count(*) FROM probe.attendance_records", connection);
            await Should.ThrowAsync<PostgresException>(() => command.ExecuteScalarAsync(Ct));
        }

        // An insert for tenant B carrying tenant A's id fails with a row-level security violation.
        await using (var connection = new NpgsqlConnection(rls.AppConnectionString))
        {
            await connection.OpenAsync(Ct);
            await using var transaction = await connection.BeginTransactionAsync(Ct);
            await using (var set = new NpgsqlCommand("SELECT set_config('app.tenant_id', @t, true)", connection, transaction))
            {
                set.Parameters.AddWithValue("t", tenantB.ToString("D"));
                await set.ExecuteScalarAsync(Ct);
            }

            await using var insert = new NpgsqlCommand(
                "INSERT INTO probe.attendance_records (tenant_id, id, status, created_at, updated_at) VALUES (@a, @id, 'absent', now(), now())",
                connection,
                transaction);
            insert.Parameters.AddWithValue("a", tenantA);
            insert.Parameters.AddWithValue("id", Guid.CreateVersion7());
            var refused = await Should.ThrowAsync<PostgresException>(() => insert.ExecuteNonQueryAsync(Ct));
            refused.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
            refused.MessageText.ShouldContain("row-level security");
        }
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-953")]
    public async Task With_the_ef_tenant_filter_removed_row_level_security_still_returns_no_row_of_another_tenant()
    {
        await using var provider = RowLevelSecurityFixture.BuildProvider(rls.AppConnectionString);
        var tenantA = await SeedAsync(provider, 3);
        var tenantB = await SeedAsync(provider, 2);

        var seenByB = await CountAsync(provider, tenantB, bypassEfTenantFilter: true);
        var seenByA = await CountAsync(provider, tenantA, bypassEfTenantFilter: true);

        seenByB.ShouldBe(2);
        seenByA.ShouldBe(3);
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-952")]
    public async Task The_application_role_can_neither_alter_the_table_nor_its_policy_nor_bypass_row_level_security()
    {
        var direct = rls.DirectAppConnectionString("nibras_probe");
        (await Should.ThrowAsync<PostgresException>(() =>
            RowLevelSecurityFixture.ExecuteAsync(direct, "ALTER TABLE probe.attendance_records ADD COLUMN planted int", Ct)))
            .SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
        (await Should.ThrowAsync<PostgresException>(() =>
            RowLevelSecurityFixture.ExecuteAsync(direct, "ALTER POLICY tenant_isolation ON probe.attendance_records USING (true)", Ct)))
            .SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);

        await using var connection = new NpgsqlConnection(direct);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand("SELECT rolbypassrls OR rolsuper FROM pg_roles WHERE rolname = current_user", connection);
        ((bool)(await command.ExecuteScalarAsync(Ct))!).ShouldBeFalse();
    }

    [Fact]
    [Trait("TestCase", "TC-SEC-960")]
    public async Task A_service_role_cannot_connect_to_another_service_database()
    {
        var foreign = rls.DirectAppConnectionString("nibras_other");

        var refused = await Should.ThrowAsync<PostgresException>(() => RowLevelSecurityFixture.ExecuteAsync(foreign, "SELECT 1", Ct));

        refused.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    [Trait("TestCase", "TC-PLAT-015")]
    public async Task A_session_level_SET_leaks_to_the_next_pooled_client_which_is_why_only_SET_LOCAL_is_used()
    {
        await using var provider = RowLevelSecurityFixture.BuildProvider(rls.AppConnectionString);
        var tenantA = await SeedAsync(provider, 4);

        // Client one sets the tenant for the session, the mistake the building block never makes.
        await using (var one = new NpgsqlConnection(rls.AppConnectionString))
        {
            await one.OpenAsync(Ct);
            await using var set = new NpgsqlCommand($"SET app.tenant_id = '{tenantA:D}'", one);
            await set.ExecuteNonQueryAsync(Ct);
        }

        // Client two never set a tenant, yet it is served by the same server connection and sees tenant A.
        long leaked;
        await using (var two = new NpgsqlConnection(rls.AppConnectionString))
        {
            await two.OpenAsync(Ct);
            await using var read = new NpgsqlCommand("SELECT count(*) FROM probe.attendance_records", two);
            leaked = (long)(await read.ExecuteScalarAsync(Ct))!;
            await using var reset = new NpgsqlCommand("RESET app.tenant_id", two);
            await reset.ExecuteNonQueryAsync(Ct);
        }

        leaked.ShouldBe(4, "the pool of one really is shared, so the SET LOCAL design is the only safe one");
    }
}
