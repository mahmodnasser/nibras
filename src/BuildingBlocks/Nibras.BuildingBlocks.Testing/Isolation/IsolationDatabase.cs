using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Nibras.BuildingBlocks.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Nibras.BuildingBlocks.Testing.Isolation;

/// <summary>
/// One service's database as production shapes it (document 10, parts 2.4 and 2.5): PostgreSQL 18.6 with the
/// <c>svc_</c> and <c>mig_</c> roles of <see cref="DatabaseBootstrap"/>, and PgBouncer 1.25.2 in transaction mode with
/// ONE server connection in front of it, so two requests in sequence are guaranteed to share a server connection.
/// </summary>
public sealed class IsolationDatabase : IAsyncDisposable
{
    public const string PostgresImage = "postgres:18.6";

    public const string PgBouncerImage = "edoburu/pgbouncer:v1.25.2-p0";

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly IContainer _pgbouncer;

    private IsolationDatabase(INetwork network, PostgreSqlContainer postgres, IContainer pgbouncer, string service)
    {
        _network = network;
        _postgres = postgres;
        _pgbouncer = pgbouncer;
        var database = DatabaseBootstrap.DatabaseName(service);
        var superuser = postgres.GetConnectionString();
        SuperuserConnectionString = new NpgsqlConnectionStringBuilder(superuser) { Database = database }.ConnectionString;
        MigrationConnectionString = new NpgsqlConnectionStringBuilder(superuser)
        {
            Database = database,
            Username = DatabaseBootstrap.MigrationRole(service),
            Password = MigrationPassword(service),
        }.ConnectionString;
        DirectApplicationConnectionString = new NpgsqlConnectionStringBuilder(superuser)
        {
            Database = database,
            Username = DatabaseBootstrap.ApplicationRole(service),
            Password = ApplicationPassword(service),
        }.ConnectionString;
        ApplicationConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = pgbouncer.Hostname,
            Port = pgbouncer.GetMappedPublicPort(5432),
            Database = database,
            Username = DatabaseBootstrap.ApplicationRole(service),
            Password = ApplicationPassword(service),
            NoResetOnClose = true, // PgBouncer in transaction mode resets nothing, so neither does the client
            MaxPoolSize = 4,
        }.ConnectionString;
    }

    /// <summary>The application role through PgBouncer: how the service connects in production.</summary>
    public string ApplicationConnectionString { get; }

    /// <summary>The migration role, directly: it owns the schema and creates the tables.</summary>
    public string MigrationConnectionString { get; }

    /// <summary>The superuser, directly: the suite's own reads of both tenants, which row-level security does not bind.</summary>
    public string SuperuserConnectionString { get; }

    /// <summary>
    /// The application role straight to PostgreSQL, bypassing PgBouncer: the session connection a component needs to
    /// hold an advisory lock or a <c>LISTEN</c>, which transaction pooling cannot (document 11 part 8.3).
    /// </summary>
    public string DirectApplicationConnectionString { get; }

    /// <summary>Starts the database of <paramref name="service"/>, the snake_case name its schema, database and roles share.</summary>
    public static Task<IsolationDatabase> StartAsync(string service, CancellationToken cancellationToken) => StartAsync(service, 1, cancellationToken);

    /// <summary>
    /// As <see cref="StartAsync(string, CancellationToken)"/>. The pool of one server connection is what proves
    /// isolation; a suite that holds two transactions at once asks for more.
    /// </summary>
    public static async Task<IsolationDatabase> StartAsync(string service, int serverConnections, CancellationToken cancellationToken)
    {
        var network = new NetworkBuilder().Build();
        await network.CreateAsync(cancellationToken).ConfigureAwait(false);
        var postgres = new PostgreSqlBuilder(PostgresImage).WithNetwork(network).WithNetworkAliases("db").Build();
        IContainer? pgbouncer = null;
        try
        {
            await postgres.StartAsync(cancellationToken).ConfigureAwait(false);
            var superuser = postgres.GetConnectionString();
            await ExecuteEachAsync(superuser, DatabaseBootstrap.ClusterStatements(service, ApplicationPassword(service), MigrationPassword(service)), cancellationToken)
                .ConfigureAwait(false);
            var serviceDatabase = new NpgsqlConnectionStringBuilder(superuser) { Database = DatabaseBootstrap.DatabaseName(service) }.ConnectionString;
            await ExecuteEachAsync(serviceDatabase, DatabaseBootstrap.DatabaseStatements(service), cancellationToken).ConfigureAwait(false);

            pgbouncer = new ContainerBuilder(PgBouncerImage)
                .WithNetwork(network)
                .WithEnvironment("DB_HOST", "db")
                .WithEnvironment("DB_PORT", "5432")
                .WithEnvironment("DB_NAME", DatabaseBootstrap.DatabaseName(service))
                .WithEnvironment("DB_USER", DatabaseBootstrap.ApplicationRole(service))
                .WithEnvironment("DB_PASSWORD", ApplicationPassword(service))
                .WithEnvironment("AUTH_TYPE", "scram-sha-256")
                .WithEnvironment("POOL_MODE", "transaction")
                .WithEnvironment("DEFAULT_POOL_SIZE", serverConnections.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .WithEnvironment("MAX_CLIENT_CONN", "50")
                .WithPortBinding(5432, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
                .Build();
            await pgbouncer.StartAsync(cancellationToken).ConfigureAwait(false);
            return new IsolationDatabase(network, postgres, pgbouncer, service);
        }
        catch
        {
            if (pgbouncer is not null)
            {
                await pgbouncer.DisposeAsync().ConfigureAwait(false);
            }

            await postgres.DisposeAsync().ConfigureAwait(false);
            await network.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _pgbouncer.DisposeAsync().ConfigureAwait(false);
        await _postgres.DisposeAsync().ConfigureAwait(false);
        await _network.DisposeAsync().ConfigureAwait(false);
    }

    private static string ApplicationPassword(string service) => $"svc-{service}-isolation-only";

    private static string MigrationPassword(string service) => $"mig-{service}-isolation-only";

    private static async Task ExecuteEachAsync(string connectionString, IReadOnlyList<string> statements, CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(connectionString);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            foreach (var statement in statements)
            {
                var command = new NpgsqlCommand(statement, connection);
                await using (command.ConfigureAwait(false))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
