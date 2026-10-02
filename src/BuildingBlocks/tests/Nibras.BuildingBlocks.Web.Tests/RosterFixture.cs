using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nibras.BuildingBlocks.Application.Lists;
using Nibras.BuildingBlocks.Domain;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Persistence;
using Nibras.BuildingBlocks.Tenancy;
using Nibras.BuildingBlocks.Web.Lists;
using Testcontainers.PostgreSql;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

public enum PupilStatus
{
    Enrolled,
    Suspended,
    Withdrawn,
}

/// <summary>A tenant-owned row shaped like a student list: text with ties, a nullable text, an enum, a date, an instant, money, a reference.</summary>
public sealed class RosterPupil : AggregateRoot<Guid>, ITenantEntity
{
    public RosterPupil(Guid id, string lastName, string firstName)
        : base(id)
    {
        LastName = lastName;
        FirstName = firstName;
    }

    public Guid TenantId { get; private set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string? Nickname { get; set; }

    public PupilStatus Status { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }

    public decimal Balance { get; set; }

    public Guid? SectionId { get; set; }
}

public sealed record PupilRow(Guid Id, string LastName, string FirstName, string? Nickname, PupilStatus Status, DateOnly DateOfBirth, DateTimeOffset EnrolledAt, decimal Balance, Guid? SectionId);

public sealed class RosterDbContext(DbContextOptions<RosterDbContext> options) : NibrasDbContext(options)
{
    public DbSet<RosterPupil> Pupils => Set<RosterPupil>();

    protected override string Schema => "roster";

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<RosterPupil>(pupil =>
        {
            pupil.HasKey(p => p.Id);
            pupil.Property(p => p.LastName).HasMaxLength(100);
            pupil.Property(p => p.FirstName).HasMaxLength(100);
            pupil.Property(p => p.Nickname).HasMaxLength(100);
            pupil.Property(p => p.Balance).HasPrecision(12, 2);
            pupil.Ignore(p => p.DomainEvents);
        });
}

public static class Pupils
{
    public static readonly ListFieldMap<RosterPupil> Fields = ListFieldMap.For<RosterPupil>(p => p.Id, "+lastName")
        .Field("lastName", p => p.LastName, FilterOperators.Text, sortable: true)
        .Field("firstName", p => p.FirstName, FilterOperators.Text, sortable: true)
        .Field("nickname", p => p.Nickname, FilterOperators.Text | FilterOperators.IsNull, sortable: true)
        .Field("status", p => p.Status, FilterOperators.Equality, sortable: true)
        .Field("dateOfBirth", p => p.DateOfBirth, FilterOperators.Range, sortable: true)
        .Field("enrolledAt", p => p.EnrolledAt, FilterOperators.Range, sortable: true)
        .Field("balance", p => p.Balance, FilterOperators.Range, sortable: true)
        .Field("sectionId", p => p.SectionId, FilterOperators.Equality | FilterOperators.IsNull);

    public static readonly System.Linq.Expressions.Expression<Func<RosterPupil, PupilRow>> ToRow = p =>
        new PupilRow(p.Id, p.LastName, p.FirstName, p.Nickname, p.Status, p.DateOfBirth, p.EnrolledAt, p.Balance, p.SectionId);
}

/// <summary>
/// PostgreSQL 18.6 with 1,001 pupils of tenant A and 20 of tenant B, and the probe service serving them as a
/// keyset list through the Web block, as a service's endpoint would.
/// </summary>
public sealed class RosterFixture : IAsyncLifetime
{
    public const int TenantACount = 1001;

    public static readonly Guid TenantA = Guid.Parse("018f0000-0000-7000-8000-00000000000a");
    public static readonly Guid TenantB = Guid.Parse("018f0000-0000-7000-8000-00000000000b");
    public static readonly Guid[] Sections = [Guid.Parse("018f1111-0000-7000-8000-000000000001"), Guid.Parse("018f1111-0000-7000-8000-000000000002")];

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.6").Build();

    public WebApplication App { get; private set; } = null!;

    public IReadOnlyList<RosterPupil> Seeded { get; private set; } = [];

    public HttpClient Client()
    {
        var client = App.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Nibras-Tenant-Id", TenantA.ToString("D"));
        return client;
    }

    public AsyncServiceScope ScopeFor(Guid tenant)
    {
        var scope = App.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
        return scope;
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration[ListCursors.KeySetting] = ProbeApi.CursorKey;
        builder.AddNibrasTelemetry("Probe");
        builder.Services.AddNibrasWeb("Probe");
        builder.Services.AddNibrasDbContext<RosterDbContext>(_postgres.GetConnectionString(), poolSize: 8);

        App = builder.Build();
        App.UseNibrasTelemetry();
        App.UseNibrasWeb();
        App.Use((context, next) =>
        {
            if (Guid.TryParse(context.Request.Headers["X-Nibras-Tenant-Id"], out var tenant))
            {
                context.RequestServices.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
            }

            return next(context);
        });

        var api = App.MapNibrasApi(1);
        api.MapGet("/pupils", (HttpContext http, RosterDbContext db) =>
            PaginationEnvelope.KeysetAsync(http, Pupils.Fields, (request, ct) => db.Pupils.ToKeysetPageAsync(request, Pupils.Fields, Pupils.ToRow, ct)));
        api.MapGet("/archived-pupils", (HttpContext http, RosterDbContext db) =>
            PaginationEnvelope.KeysetAsync(http, Pupils.Fields, (request, ct) => db.Pupils.ToKeysetPageAsync(request, Pupils.Fields, Pupils.ToRow, ct)));

        await App.StartAsync();
        await SeedAsync();
    }

    private async Task SeedAsync()
    {
        await using (var scope = App.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<RosterDbContext>().Database.EnsureCreatedAsync();
        }

        var seeded = new List<RosterPupil>();
        foreach (var (tenant, count) in new[] { (TenantA, TenantACount), (TenantB, 20) })
        {
            await using var scope = ScopeFor(tenant);
            var db = scope.ServiceProvider.GetRequiredService<RosterDbContext>();
            for (var i = 0; i < count; i++)
            {
                var pupil = new RosterPupil(Guid.CreateVersion7(), "family" + (i % 97).ToString("D3", System.Globalization.CultureInfo.InvariantCulture), "given" + (i % 13).ToString("D2", System.Globalization.CultureInfo.InvariantCulture))
                {
                    Nickname = i % 4 == 0 ? null : "nick" + (i % 50).ToString("D2", System.Globalization.CultureInfo.InvariantCulture),
                    Status = (PupilStatus)(i % 3),
                    DateOfBirth = new DateOnly(2014, 1, 1).AddDays(i % 365),
                    EnrolledAt = new DateTimeOffset(2026, 9, 1, 5, 0, 0, TimeSpan.Zero).AddMinutes(i % 600).AddTicks(i * 10),
                    Balance = (i * 7 % 500) + 0.25m,
                    SectionId = i % 5 == 0 ? null : Sections[i % 2],
                };
                db.Pupils.Add(pupil);
                if (tenant == TenantA)
                {
                    seeded.Add(pupil);
                }
            }

            await db.SaveChangesAsync();
        }

        Seeded = seeded;
    }

    public async ValueTask DisposeAsync()
    {
        await App.StopAsync();
        await App.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class RosterDefinition : ICollectionFixture<RosterFixture>
{
    public const string Name = "roster";
}
