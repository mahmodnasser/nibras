using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// The row-level security contract of document 10 part 2.4 and reference architecture Section 14: every
/// tenant-owned table has RLS enabled and forced, and one policy that compares <c>tenant_id</c> with the
/// transaction-local <c>app.tenant_id</c> setting, both for reading and for writing.
/// </summary>
public static class RowLevelSecurity
{
    public const string PolicyName = "tenant_isolation";

    public const string TenantSetting = "app.tenant_id";

    /// <summary>The statements for one table. <c>current_setting</c> without a default raises when the setting is unset.</summary>
    public static string PolicySql(string schema, string table)
    {
        var qualified = Qualify(schema, table);
        return string.Create(CultureInfo.InvariantCulture, $"""
            ALTER TABLE {qualified} ENABLE ROW LEVEL SECURITY;
            ALTER TABLE {qualified} FORCE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS {PolicyName} ON {qualified};
            CREATE POLICY {PolicyName} ON {qualified}
              USING (tenant_id = current_setting('{TenantSetting}')::uuid)
              WITH CHECK (tenant_id = current_setting('{TenantSetting}')::uuid);
            """);
    }

    /// <summary>The statements for every tenant-owned table of a model, in a stable order.</summary>
    public static string PolicySqlFor(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sql = new StringBuilder();
        foreach (var entity in context.Model.GetEntityTypes()
                     .Where(e => typeof(ITenantEntity).IsAssignableFrom(e.ClrType) && e.GetTableName() is not null)
                     .OrderBy(e => e.GetTableName(), StringComparer.Ordinal))
        {
            sql.AppendLine(PolicySql(entity.GetSchema() ?? "public", entity.GetTableName()!));
        }

        return sql.ToString();
    }

    /// <summary>Adds the policy of one table to a migration, in the same migration that creates the table.</summary>
    public static MigrationBuilder EnableTenantRowLevelSecurity(this MigrationBuilder migrationBuilder, string table, string schema)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(PolicySql(schema, table));
        return migrationBuilder;
    }

    private static string Qualify(string schema, string table)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);
        return $"{Identifier(schema)}.{Identifier(table)}";
    }

    internal static string Identifier(string name) =>
        name.All(c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_') && name.Length > 0 && name[0] is (>= 'a' and <= 'z') or '_'
            ? name
            : throw new ArgumentException($"'{name}' is not a snake_case identifier.", nameof(name));
}

/// <summary>
/// The database, schema and the two roles of one service (REQ-DATA-002, REQ-SEC-010): <c>mig_&lt;service&gt;</c>
/// owns the schema and every table; <c>svc_&lt;service&gt;</c> is the application role, owns nothing, and has no
/// <c>BYPASSRLS</c>. No other role may connect to the database. Passwords come from the secret store, never
/// from this repository.
/// </summary>
public static class DatabaseBootstrap
{
    public static string DatabaseName(string service) => "nibras_" + RowLevelSecurity.Identifier(service);

    public static string ApplicationRole(string service) => "svc_" + RowLevelSecurity.Identifier(service);

    public static string MigrationRole(string service) => "mig_" + RowLevelSecurity.Identifier(service);

    /// <summary>
    /// Run as a superuser against the maintenance database, one statement at a time: <c>CREATE DATABASE</c>
    /// cannot run inside a transaction block.
    /// </summary>
    public static IReadOnlyList<string> ClusterStatements(string service, string applicationPassword, string migrationPassword)
    {
        var database = DatabaseName(service);
        var app = ApplicationRole(service);
        var mig = MigrationRole(service);
        return
        [
            $"CREATE ROLE {mig} LOGIN PASSWORD {Literal(migrationPassword)} NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS",
            $"CREATE ROLE {app} LOGIN PASSWORD {Literal(applicationPassword)} NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS NOINHERIT",
            $"CREATE DATABASE {database} OWNER {mig}",
            $"REVOKE ALL ON DATABASE {database} FROM PUBLIC",
            $"GRANT CONNECT ON DATABASE {database} TO {mig}, {app}",
        ];
    }

    /// <summary>Run as a superuser connected to the service's own database, one statement at a time.</summary>
    public static IReadOnlyList<string> DatabaseStatements(string service)
    {
        var schema = RowLevelSecurity.Identifier(service);
        var app = ApplicationRole(service);
        var mig = MigrationRole(service);
        return
        [
            "REVOKE CREATE ON SCHEMA public FROM PUBLIC",
            $"CREATE SCHEMA {schema} AUTHORIZATION {mig}",
            $"GRANT USAGE ON SCHEMA {schema} TO {app}",
            $"ALTER DEFAULT PRIVILEGES FOR ROLE {mig} IN SCHEMA {schema} GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {app}",
            $"ALTER DEFAULT PRIVILEGES FOR ROLE {mig} IN SCHEMA {schema} GRANT USAGE, SELECT ON SEQUENCES TO {app}",
        ];
    }

    private static string Literal(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
    }
}
