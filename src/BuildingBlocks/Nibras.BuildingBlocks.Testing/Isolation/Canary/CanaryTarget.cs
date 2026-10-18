using System.Globalization;
using System.Linq.Expressions;
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
using Nibras.BuildingBlocks.Web;
using Nibras.BuildingBlocks.Web.Lists;
using Nibras.Contracts.Shared;

namespace Nibras.BuildingBlocks.Testing.Isolation.Canary;

public sealed class CanarySection : AggregateRoot<Guid>, ITenantEntity
{
    public CanarySection(Guid id, string name)
        : base(id) => Name = name;

    public Guid TenantId { get; private set; }

    public string Name { get; private set; }

    public bool Closed { get; private set; }
}

public sealed class CanaryPupil : AggregateRoot<Guid>, ITenantEntity
{
    public CanaryPupil(Guid id, string name, Guid sectionId)
        : base(id)
    {
        Name = name;
        SectionId = sectionId;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; }

    public Guid SectionId { get; private set; }

    public void Rename(string name) => Name = name;
}

public sealed class CanaryDbContext(DbContextOptions<CanaryDbContext> options) : NibrasDbContext(options)
{
    public DbSet<CanarySection> Sections => Set<CanarySection>();

    public DbSet<CanaryPupil> Pupils => Set<CanaryPupil>();

    protected override string Schema => "canary";

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<CanarySection>(section =>
        {
            section.ToTable("sections");
            section.HasKey(s => s.Id);
            section.Property(s => s.Name).HasMaxLength(100);
            section.Ignore(s => s.DomainEvents);
        });
        modelBuilder.Entity<CanaryPupil>(pupil =>
        {
            pupil.ToTable("pupils");
            pupil.HasKey(p => p.Id);
            pupil.Property(p => p.Name).HasMaxLength(100);
            pupil.Ignore(p => p.DomainEvents);
        });
    }
}

public sealed record CanaryPupilView(Guid Id, string Name, Guid SectionId);

public sealed record CreateCanaryPupil(string Name, Guid SectionId);

public sealed record RenameCanaryPupil(string Name);

/// <summary>
/// The canary: a small service with the shapes a real service has, built the way the plan builds one. It has a
/// keyset list, a read, an update and a delete under <c>If-Match</c>, a create that references another resource, a
/// sub-collection and a consumer. The suite attacks it on every build, so a run that attacks nothing, or an engine
/// that has stopped catching a leak, shows. The block's own tests derive from it to plant defects.
/// </summary>
public class CanaryTarget : IsolationTarget<CanaryDbContext>
{
    public const string SectionClosed = "canary.section.closed.v1";

    private static readonly ListFieldMap<CanaryPupil> Fields = ListFieldMap.For<CanaryPupil>(p => p.Id, "+name")
        .Field("name", p => p.Name, FilterOperators.Text, sortable: true)
        .Field("sectionId", p => p.SectionId, FilterOperators.Equality);

    private static readonly Expression<Func<CanaryPupil, CanaryPupilView>> ToView = p => new CanaryPupilView(p.Id, p.Name, p.SectionId);

    public override string Service => "Canary";

    public override IReadOnlyList<IsolationConsumer> Consumers => [new(SectionClosed, "sections", CloseSectionAsync)];

    public override WebApplication BuildHost(string connectionString)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = "Test" });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddNibrasTelemetry(Service);
        builder.Services.AddNibrasWeb(Service);
        builder.Services.AddNibrasDbContext<CanaryDbContext>(connectionString, poolSize: 4);

        var app = builder.Build();
        app.UseNibrasTelemetry();
        app.UseNibrasWeb();
        app.Use((context, next) =>
        {
            // Stands in for the Gateway-resolved tenant, which the identity slices wire.
            if (Guid.TryParse(context.Request.Headers[TenantHeader], out var tenant))
            {
                context.RequestServices.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
            }

            return next(context);
        });

        var api = app.MapNibrasApi(1);
        api.MapGet("/pupils", (HttpContext http, CanaryDbContext db) => PaginationEnvelope.KeysetAsync(
            http, Fields, (request, ct) => db.InTenantTransactionAsync(c => Pupils(db).ToKeysetPageAsync(request, Fields, ToView, c), ct)));
        api.MapGet("/pupils/{id:guid}", GetPupilAsync);
        api.MapPut("/pupils/{id:guid}", RenamePupilAsync);
        api.MapDelete("/pupils/{id:guid}", DeletePupilAsync);
        api.MapPost("/pupils", CreatePupilAsync);
        api.MapGet("/sections/{sectionId:guid}/pupils", SectionPupilsAsync);
        return app;
    }

    public override async Task SeedAsync(IServiceProvider scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var db = scope.GetRequiredService<CanaryDbContext>();
        for (var s = 1; s <= 2; s++)
        {
            var section = new CanarySection(Guid.CreateVersion7(), string.Create(CultureInfo.InvariantCulture, $"Section {s}"));
            db.Sections.Add(section);
            for (var p = 1; p <= 3; p++)
            {
                db.Pupils.Add(new CanaryPupil(Guid.CreateVersion7(), string.Create(CultureInfo.InvariantCulture, $"Pupil {s}.{p}"), section.Id));
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>The pupils a handler reads. A planted defect overrides it.</summary>
    protected virtual IQueryable<CanaryPupil> Pupils(CanaryDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        return db.Pupils;
    }

    /// <summary>The sections a handler or the consumer reads. A planted defect overrides it.</summary>
    protected virtual IQueryable<CanarySection> Sections(CanaryDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        return db.Sections;
    }

    private async Task<IResult> GetPupilAsync(Guid id, CanaryDbContext db, CancellationToken cancellationToken)
    {
        var pupil = await db.InTenantTransactionAsync(ct => Pupils(db).FirstOrDefaultAsync(p => p.Id == id, ct), cancellationToken).ConfigureAwait(false);
        return pupil is null ? NotFound("pupil", id) : (IResult)NibrasResults.Versioned(View(pupil), pupil.Id, db.RowVersionOf(pupil));
    }

    private Task<IResult> RenamePupilAsync(Guid id, IfMatch ifMatch, RenameCanaryPupil body, CanaryDbContext db, CancellationToken cancellationToken) =>
        db.InTenantTransactionAsync(
            async Task<IResult> (ct) =>
            {
                var pupil = await Pupils(db).FirstOrDefaultAsync(p => p.Id == id, ct).ConfigureAwait(false);
                if (pupil is null)
                {
                    return NotFound("pupil", id);
                }

                var version = db.RowVersionOf(pupil);
                if (!ifMatch.Matches(pupil.Id, version))
                {
                    return NibrasResults.ConcurrencyConflict(ETags.For(version, pupil.Id));
                }

                pupil.Rename(body.Name);
                db.ExpectRowVersion(pupil, version);
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                return NibrasResults.Versioned(View(pupil), pupil.Id, db.RowVersionOf(pupil));
            },
            cancellationToken);

    private Task<IResult> DeletePupilAsync(Guid id, IfMatch ifMatch, CanaryDbContext db, CancellationToken cancellationToken) =>
        db.InTenantTransactionAsync(
            async Task<IResult> (ct) =>
            {
                var pupil = await Pupils(db).FirstOrDefaultAsync(p => p.Id == id, ct).ConfigureAwait(false);
                if (pupil is null)
                {
                    return NotFound("pupil", id);
                }

                var version = db.RowVersionOf(pupil);
                if (!ifMatch.Matches(pupil.Id, version))
                {
                    return NibrasResults.ConcurrencyConflict(ETags.For(version, pupil.Id));
                }

                db.Pupils.Remove(pupil);
                db.ExpectRowVersion(pupil, version);
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                return TypedResults.NoContent();
            },
            cancellationToken);

    private Task<IResult> CreatePupilAsync(CreateCanaryPupil body, CanaryDbContext db, CancellationToken cancellationToken) =>
        db.InTenantTransactionAsync(
            async Task<IResult> (ct) =>
            {
                if (!await Sections(db).AnyAsync(s => s.Id == body.SectionId, ct).ConfigureAwait(false))
                {
                    return NotFound("section", body.SectionId);
                }

                var pupil = new CanaryPupil(Guid.CreateVersion7(), body.Name, body.SectionId);
                db.Pupils.Add(pupil);
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                return TypedResults.Created(new Uri("/api/v1/canary/pupils/" + pupil.Id.ToString("D"), UriKind.Relative), View(pupil));
            },
            cancellationToken);

    private Task<IResult> SectionPupilsAsync(Guid sectionId, CanaryDbContext db, CancellationToken cancellationToken) =>
        db.InTenantTransactionAsync(
            async Task<IResult> (ct) =>
            {
                if (!await Sections(db).AnyAsync(s => s.Id == sectionId, ct).ConfigureAwait(false))
                {
                    return NotFound("section", sectionId);
                }

                var pupils = await Pupils(db).Where(p => p.SectionId == sectionId).OrderBy(p => p.Id).Take(100).Select(ToView).ToListAsync(ct).ConfigureAwait(false);
                return TypedResults.Ok(pupils);
            },
            cancellationToken);

    /// <summary>
    /// Closes a section under the envelope's tenant, as the consumer host sets it; an unknown section is rejected. It
    /// writes with <c>ExecuteUpdate</c>, as document 21 asks of bulk writes, which skips the save-time tenant check, so
    /// only the tenant filter and row-level security stand between it and another tenant's row.
    /// </summary>
    private async Task CloseSectionAsync(IServiceProvider services, MessageEnvelope envelope, Guid sectionId, CancellationToken cancellationToken)
    {
        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var tenant = envelope.TenantId ?? throw new InvalidOperationException($"{SectionClosed} is tenant-owned; its envelope must carry a tenant.");
            scope.ServiceProvider.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
            var db = scope.ServiceProvider.GetRequiredService<CanaryDbContext>();
            var closed = await db.InTenantTransactionAsync(
                ct => Sections(db).Where(s => s.Id == sectionId).ExecuteUpdateAsync(set => set.SetProperty(s => s.Closed, true), ct),
                cancellationToken).ConfigureAwait(false);
            if (closed == 0)
            {
                throw new InvalidOperationException($"Section {sectionId} does not exist under the envelope's tenant.");
            }
        }
    }

    private static CanaryPupilView View(CanaryPupil pupil) => new(pupil.Id, pupil.Name, pupil.SectionId);

    private static NibrasProblemResult NotFound(string resource, Guid id) =>
        new("CANARY_" + ErrorCatalog.NotFound, $"No {resource} {id:D} in this school.");
}
