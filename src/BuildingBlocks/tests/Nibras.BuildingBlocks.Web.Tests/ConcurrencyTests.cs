using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Domain;
using Nibras.BuildingBlocks.Persistence;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>An aggregate root with the conventions of SL-DATA-001, used only by these tests.</summary>
public sealed class Pupil : AggregateRoot<Guid>, ITenantEntity
{
    public Pupil(Guid id, string fullName)
        : base(id) => FullName = fullName;

    public Guid TenantId { get; private set; }

    public string FullName { get; set; }
}

public sealed class PupilsDbContext(DbContextOptions<PupilsDbContext> options) : NibrasDbContext(options)
{
    public DbSet<Pupil> Pupils => Set<Pupil>();

    protected override string Schema => "web_test";

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Pupil>(pupil =>
        {
            pupil.HasKey(p => p.Id);
            pupil.Property(p => p.FullName).HasMaxLength(200);
            pupil.Ignore(p => p.DomainEvents);
        });
}

public sealed record RenamePupil(string FullName);

public sealed record PupilResponse(Guid Id, string FullName);

[JsonSerializable(typeof(RenamePupil))]
[JsonSerializable(typeof(PupilResponse))]
internal sealed partial class PupilJsonContext : JsonSerializerContext;

/// <summary>One PostgreSQL 18.6 container for the concurrency tests, with the schema created once.</summary>
public sealed class PupilsDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        var services = new ServiceCollection();
        services.AddNibrasDbContext<PupilsDbContext>(ConnectionString, poolSize: 2);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<PupilsDbContext>().Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

/// <summary>
/// TC-API-020 (REQ-API-014, REQ-PERF-018) and TC-API-021 (REQ-API-015): an update names the version it read in
/// <c>If-Match</c>; without it the update is refused, and with a stale tag it is 409 with the current tag.
/// </summary>
public sealed class ConcurrencyTests(PupilsDatabase database) : IClassFixture<PupilsDatabase>
{
    private static readonly Guid Tenant = Guid.CreateVersion7();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>The handler shape a service writes for an aggregate update: check the tag, expect the version, save.</summary>
    private static async Task<IResult> RenameAsync(Guid id, RenamePupil body, IfMatch ifMatch, PupilsDbContext db, CancellationToken ct)
    {
        var pupil = await db.Pupils.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (pupil is null)
        {
            return Result.Failure(new Error("PROBE_NOT_FOUND", "No such pupil.")).ToProblem();
        }

        if (!ifMatch.TryGetRowVersion(id, out var expected))
        {
            return NibrasResults.ConcurrencyConflict(ETags.For(db.RowVersionOf(pupil), id), ["fullName"]);
        }

        db.ExpectRowVersion(pupil, expected);
        pupil.FullName = body.FullName;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await db.Entry(pupil).ReloadAsync(ct);
            return NibrasResults.ConcurrencyConflict(ETags.For(db.RowVersionOf(pupil), id), ["fullName"]);
        }

        return NibrasResults.Versioned(new PupilResponse(id, pupil.FullName), id, db.RowVersionOf(pupil));
    }

    private Task<ProbeApi> StartAsync() => ProbeApi.StartAsync(
        configure: builder =>
        {
            builder.Services.AddNibrasDbContext<PupilsDbContext>(database.ConnectionString, poolSize: 4);
            builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, PupilJsonContext.Default));
        },
        extraRoutes: app =>
        {
            var api = app.MapNibrasApi(1);
            api.MapGet("/pupils/{id:guid}", async (Guid id, PupilsDbContext db, CancellationToken ct) =>
            {
                var pupil = await db.Pupils.SingleAsync(p => p.Id == id, ct);
                return NibrasResults.Versioned(new PupilResponse(id, pupil.FullName), id, db.RowVersionOf(pupil));
            });
            api.MapPatch("/pupils/{id:guid}", RenameAsync);
            api.MapPut("/pupils/{id:guid}", RenameAsync);
            api.MapDelete("/pupils/{id:guid}", (Guid id, IfMatch ifMatch) => TypedResults.Ok(new PupilResponse(id, ifMatch.IsAny ? "*" : "tagged")));
            api.MapPut("/pupils/{id:guid}/draft", (Guid id) => TypedResults.NoContent()).WithoutIfMatch();
        });

    private async Task<Guid> SeedAsync(string name)
    {
        var services = new ServiceCollection();
        services.AddNibrasDbContext<PupilsDbContext>(database.ConnectionString, poolSize: 1);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<Tenancy.TenantContext>().Set(new Tenancy.TenantId(Tenant));
        var db = scope.ServiceProvider.GetRequiredService<PupilsDbContext>();
        var id = Guid.CreateVersion7();
        db.Pupils.Add(new Pupil(id, name));
        await db.SaveChangesAsync(Ct);
        return id;
    }

    private async Task<string> StoredNameAsync(Guid id)
    {
        var services = new ServiceCollection();
        services.AddNibrasDbContext<PupilsDbContext>(database.ConnectionString, poolSize: 1);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<Tenancy.TenantContext>().Set(new Tenancy.TenantId(Tenant));
        var db = scope.ServiceProvider.GetRequiredService<PupilsDbContext>();
        return await db.Pupils.AsNoTracking().Where(p => p.Id == id).Select(p => p.FullName).SingleAsync(Ct);
    }

    private static HttpRequestMessage Request(HttpMethod method, string path, string? ifMatch, object? body = null)
    {
        var request = new HttpRequestMessage(method, new Uri(path, UriKind.Relative));
        request.Headers.Add("X-Nibras-Tenant-Id", Tenant.ToString("D"));
        if (ifMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static async Task<JsonElement> BodyAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)).RootElement.Clone();

    [Fact]
    [Trait("TestCase", "TC-API-020")]
    public async Task Two_clients_writing_from_the_same_tag_get_200_then_409_with_the_first_writes_tag()
    {
        var id = await SeedAsync("Layla Haddad");
        await using var api = await StartAsync();
        using var client = api.Client();
        using var read = Request(HttpMethod.Get, $"/api/v1/probe/pupils/{id}", ifMatch: null);
        using var readResponse = await client.SendAsync(read, Ct);
        var tag = readResponse.Headers.ETag!.Tag;

        using var first = Request(HttpMethod.Patch, $"/api/v1/probe/pupils/{id}", tag, new { fullName = "Layla Haddad-Nasser" });
        using var firstResponse = await client.SendAsync(first, Ct);
        using var second = Request(HttpMethod.Patch, $"/api/v1/probe/pupils/{id}", tag, new { fullName = "Layla H." });
        using var secondResponse = await client.SendAsync(second, Ct);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var firstWriteTag = firstResponse.Headers.ETag!.Tag;
        firstWriteTag.ShouldNotBe(tag);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await BodyAsync(secondResponse);
        problem.GetProperty("code").GetString().ShouldBe("PROBE_CONCURRENCY_CONFLICT");
        problem.GetProperty("params").GetProperty("currentEtag").GetString().ShouldBe(firstWriteTag);
        problem.GetProperty("params").GetProperty("changedFields")[0].GetString().ShouldBe("fullName");
        (await StoredNameAsync(id)).ShouldBe("Layla Haddad-Nasser");
    }

    [Fact]
    [Trait("TestCase", "TC-API-020")]
    public async Task A_tag_of_another_row_with_the_same_version_never_matches()
    {
        var id = await SeedAsync("Omar Saleh");
        var other = await SeedAsync("Huda Saleh");
        await using var api = await StartAsync();
        using var client = api.Client();
        using var read = Request(HttpMethod.Get, $"/api/v1/probe/pupils/{other}", ifMatch: null);
        using var readResponse = await client.SendAsync(read, Ct);

        using var update = Request(HttpMethod.Put, $"/api/v1/probe/pupils/{id}", readResponse.Headers.ETag!.Tag, new { fullName = "Omar S." });
        using var response = await client.SendAsync(update, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await StoredNameAsync(id)).ShouldBe("Omar Saleh");
    }

    [Fact]
    [Trait("TestCase", "TC-API-020")]
    public async Task A_save_over_a_version_changed_after_the_read_fails_on_xmin_even_inside_one_handler()
    {
        // REQ-PERF-018: the guard is the database's, not only the tag comparison before the save.
        var id = await SeedAsync("Sara Amin");
        var services = new ServiceCollection();
        services.AddNibrasDbContext<PupilsDbContext>(database.ConnectionString, poolSize: 2);
        await using var provider = services.BuildServiceProvider();
        await using var reader = provider.CreateAsyncScope();
        await using var writer = provider.CreateAsyncScope();
        foreach (var scope in new[] { reader, writer })
        {
            scope.ServiceProvider.GetRequiredService<Tenancy.TenantContext>().Set(new Tenancy.TenantId(Tenant));
        }

        var stale = reader.ServiceProvider.GetRequiredService<PupilsDbContext>();
        var pupil = await stale.Pupils.SingleAsync(p => p.Id == id, Ct);
        var readVersion = stale.RowVersionOf(pupil);

        var other = writer.ServiceProvider.GetRequiredService<PupilsDbContext>();
        (await other.Pupils.SingleAsync(p => p.Id == id, Ct)).FullName = "Sara Amin (guardian edit)";
        await other.SaveChangesAsync(Ct);

        stale.ExpectRowVersion(pupil, readVersion);
        pupil.FullName = "Sara A.";
        await Should.ThrowAsync<DbUpdateConcurrencyException>(() => stale.SaveChangesAsync(Ct));
        (await StoredNameAsync(id)).ShouldBe("Sara Amin (guardian edit)");
    }

    [Theory]
    [Trait("TestCase", "TC-API-021")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task An_update_without_If_Match_is_400_with_the_field_and_the_row_is_unchanged(string method)
    {
        var id = await SeedAsync("Yousef Karim");
        await using var api = await StartAsync();
        using var client = api.Client();
        using var request = Request(new HttpMethod(method), $"/api/v1/probe/pupils/{id}", ifMatch: null, method == "DELETE" ? null : new { fullName = "Y. Karim" });

        using var response = await client.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await BodyAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        problem.GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("If-Match");
        problem.GetProperty("errors")[0].GetProperty("code").GetString().ShouldBe("required");
        (await StoredNameAsync(id)).ShouldBe("Yousef Karim");
    }

    [Theory]
    [Trait("TestCase", "TC-API-021")]
    [InlineData("PUT", "*", "wildcardNotAllowed")]
    [InlineData("PATCH", "W/\"7-0a1b2c3d\"", "weakTag")]
    [InlineData("PUT", "7-0a1b2c3d", "invalid")]
    public async Task A_wildcard_on_a_write_or_a_weak_or_unquoted_tag_is_refused(string method, string ifMatch, string reason)
    {
        var id = await SeedAsync("Mona Fares");
        await using var api = await StartAsync();
        using var client = api.Client();
        using var request = Request(new HttpMethod(method), $"/api/v1/probe/pupils/{id}", ifMatch, new { fullName = "M. Fares" });

        using var response = await client.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await BodyAsync(response)).GetProperty("errors")[0].GetProperty("code").GetString().ShouldBe(reason);
        (await StoredNameAsync(id)).ShouldBe("Mona Fares");
    }

    [Fact]
    [Trait("TestCase", "TC-API-021")]
    public async Task A_delete_accepts_the_wildcard_and_an_exempt_endpoint_needs_no_tag()
    {
        await using var api = await StartAsync();
        using var client = api.Client();
        var id = Guid.CreateVersion7();
        using var delete = Request(HttpMethod.Delete, $"/api/v1/probe/pupils/{id}", "*");
        using var draft = Request(HttpMethod.Put, $"/api/v1/probe/pupils/{id}/draft", ifMatch: null);

        using var deleteResponse = await client.SendAsync(delete, Ct);
        using var draftResponse = await client.SendAsync(draft, Ct);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await BodyAsync(deleteResponse)).GetProperty("fullName").GetString().ShouldBe("*");
        draftResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
