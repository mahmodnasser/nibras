using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Domain;
using Nibras.BuildingBlocks.Tenancy;
using Testcontainers.PostgreSql;
using Xunit;

namespace Nibras.BuildingBlocks.Persistence.Tests;

/// <summary>A tenant-owned entity used only by these tests.</summary>
public sealed class Student : AggregateRoot<Guid>, ITenantEntity
{
    public Student(Guid id, string fullName)
        : base(id) => FullName = fullName;

    public Guid TenantId { get; private set; }

    public string FullName { get; set; }
}

public sealed class SchoolTestDbContext(DbContextOptions<SchoolTestDbContext> options) : NibrasDbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Student>(student =>
        {
            student.HasKey(s => s.Id);
            student.Property(s => s.FullName).HasMaxLength(200);
            student.Ignore(s => s.DomainEvents);
        });
}

/// <summary>A clock the test moves by hand.</summary>
public sealed class FakeClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

/// <summary>One PostgreSQL 18.6 container for the whole assembly, with the schema created once.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6").Build();

    public string ConnectionString => _container.GetConnectionString();

    public FakeClock Clock { get; } = new(new DateTimeOffset(2026, 9, 27, 7, 30, 0, TimeSpan.Zero));

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync().ConfigureAwait(false);
        await using var provider = BuildProvider(poolSize: 4);
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<SchoolTestDbContext>().Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync().ConfigureAwait(false);

    public ServiceProvider BuildProvider(int poolSize)
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Clock);
        services.AddNibrasDbContext<SchoolTestDbContext>(ConnectionString, poolSize);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    /// <summary>Opens a scope whose tenant is set, as the host does once per request or message.</summary>
    public static AsyncServiceScope ScopeFor(ServiceProvider provider, Guid? tenant)
    {
        var scope = provider.CreateAsyncScope();
        if (tenant is { } id)
        {
            scope.ServiceProvider.GetRequiredService<TenantContext>().Set(new TenantId(id));
        }

        return scope;
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresDefinition : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
