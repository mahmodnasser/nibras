using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Domain;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Nibras.BuildingBlocks.Persistence.Tests;

/// <summary>A tenant-owned table of the throwaway service "probe".</summary>
public sealed class AttendanceRecord : AggregateRoot<Guid>, ITenantEntity
{
    public AttendanceRecord(Guid id, string status)
        : base(id) => Status = status;

    public Guid TenantId { get; private set; }

    public string Status { get; set; }
}

public sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options) : NibrasDbContext(options)
{
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override string Schema => "probe";

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<AttendanceRecord>(record =>
        {
            record.HasKey(r => r.Id);
            record.Property(r => r.Status).HasMaxLength(20);
            record.Ignore(r => r.DomainEvents);
        });
}

/// <summary>
/// PostgreSQL 18.6 with the roles of <see cref="DatabaseBootstrap"/>, tables owned by <c>mig_probe</c> with the
/// row-level security policy, and PgBouncer 1.25.2 in transaction mode with ONE server connection in front of it,
/// so two clients in sequence are guaranteed to share a server connection (document 10, part 2.5).
/// </summary>
public sealed class RowLevelSecurityFixture : IAsyncLifetime
{
    public const string AppPassword = "svc-probe-test-only";
    public const string MigPassword = "mig-probe-test-only";
    public const string OtherAppPassword = "svc-other-test-only";

    private readonly INetwork _network = new NetworkBuilder().Build();
    private PostgreSqlContainer? _postgres;
    private IContainer? _pgbouncer;

    /// <summary>The application role through PgBouncer: how a service connects in production.</summary>
    public string AppConnectionString { get; private set; } = "";

    /// <summary>The application role straight to PostgreSQL, for the tests that need a fresh session.</summary>
    public string DirectAppConnectionString(string database) =>
        new NpgsqlConnectionStringBuilder(_postgres!.GetConnectionString()) { Database = database, Username = "svc_probe", Password = AppPassword }.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await _network.CreateAsync().ConfigureAwait(false);
        _postgres = new PostgreSqlBuilder("postgres:18.6").WithNetwork(_network).WithNetworkAliases("db").Build();
        await _postgres.StartAsync().ConfigureAwait(false);

        var superuser = _postgres.GetConnectionString();
        await ExecuteEachAsync(superuser, DatabaseBootstrap.ClusterStatements("probe", AppPassword, MigPassword)).ConfigureAwait(false);
        await ExecuteEachAsync(superuser, DatabaseBootstrap.ClusterStatements("other", OtherAppPassword, "mig-other-test-only")).ConfigureAwait(false);
        await ExecuteEachAsync(With(superuser, "nibras_probe"), DatabaseBootstrap.DatabaseStatements("probe")).ConfigureAwait(false);

        // Tables are created and secured by the migration role, which owns them.
        var migration = new NpgsqlConnectionStringBuilder(superuser) { Database = "nibras_probe", Username = "mig_probe", Password = MigPassword }.ConnectionString;
        await using (var provider = BuildProvider(migration))
        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
            await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
            await ExecuteAsync(migration, RowLevelSecurity.PolicySqlFor(context)).ConfigureAwait(false);
        }

        _pgbouncer = new ContainerBuilder("edoburu/pgbouncer:v1.25.2-p0")
            .WithNetwork(_network)
            .WithEnvironment("DB_HOST", "db")
            .WithEnvironment("DB_PORT", "5432")
            .WithEnvironment("DB_NAME", "nibras_probe")
            .WithEnvironment("DB_USER", "svc_probe")
            .WithEnvironment("DB_PASSWORD", AppPassword)
            .WithEnvironment("AUTH_TYPE", "scram-sha-256")
            .WithEnvironment("POOL_MODE", "transaction")
            .WithEnvironment("DEFAULT_POOL_SIZE", "1")
            .WithEnvironment("MAX_CLIENT_CONN", "50")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();
        await _pgbouncer.StartAsync().ConfigureAwait(false);

        AppConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = _pgbouncer.Hostname,
            Port = _pgbouncer.GetMappedPublicPort(5432),
            Database = "nibras_probe",
            Username = "svc_probe",
            Password = AppPassword,
            NoResetOnClose = true, // PgBouncer resets nothing in transaction mode; DISCARD ALL is not sent
            MaxPoolSize = 4,
        }.ConnectionString;
    }

    public async ValueTask DisposeAsync()
    {
        if (_pgbouncer is not null)
        {
            await _pgbouncer.DisposeAsync().ConfigureAwait(false);
        }

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync().ConfigureAwait(false);
        }

        await _network.DisposeAsync().ConfigureAwait(false);
    }

    public static ServiceProvider BuildProvider(string connectionString, int poolSize = 4)
    {
        var services = new ServiceCollection();
        services.AddNibrasDbContext<ProbeDbContext>(connectionString, poolSize);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public static async Task ExecuteAsync(string connectionString, string sql, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExecuteEachAsync(string connectionString, IReadOnlyList<string> statements)
    {
        foreach (var statement in statements)
        {
            await ExecuteAsync(connectionString, statement).ConfigureAwait(false);
        }
    }

    private static string With(string connectionString, string database) =>
        new NpgsqlConnectionStringBuilder(connectionString) { Database = database }.ConnectionString;
}

[CollectionDefinition(Name)]
public sealed class RowLevelSecurityDefinition : ICollectionFixture<RowLevelSecurityFixture>
{
    public const string Name = "row-level-security";
}
