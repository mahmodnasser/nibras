using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nibras.BuildingBlocks.Domain;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Tenancy;

namespace Nibras.BuildingBlocks.Web.Tests;

public enum EnrolmentStatus
{
    Enrolled,
    OnLeave,
}

public sealed record StudentResponse(
    Guid Id,
    string StudentNumber,
    string Name,
    EnrolmentStatus Status,
    DateTimeOffset CreatedAt,
    DateOnly DateOfBirth,
    TimeSpan LessonLength,
    string? MiddleName);

public sealed record CreateStudent(string StudentNumber, string? Name, DateOnly DateOfBirth, EnrolmentStatus Status, DateTimeOffset? EnrolledAt);

public sealed class CreateStudentValidator : AbstractValidator<CreateStudent>
{
    public CreateStudentValidator()
    {
        RuleFor(s => s.StudentNumber).NotEmpty().MaximumLength(12);
        RuleFor(s => s.Name).MaximumLength(100);
    }
}

[JsonSerializable(typeof(StudentResponse))]
[JsonSerializable(typeof(CreateStudent))]
[JsonSerializable(typeof(StudentResponse[]))]
internal sealed partial class ProbeJsonContext : JsonSerializerContext;

/// <summary>A throwaway service "Probe" hosted in memory with the Web block exactly as a service's Program.cs wires it.</summary>
public sealed class ProbeApi : IAsyncDisposable
{
    public static readonly Guid StudentId = Guid.Parse("018f6a1e-5c2b-7d3e-9a4f-1b2c3d4e5f60");
    public static readonly Guid TenantHeaderValue = Guid.Parse("018f0000-0000-7000-8000-000000000001");

    private ProbeApi(WebApplication app) => App = app;

    public WebApplication App { get; }

    private int _createCalls;
    private int _publicCalls;

    public int CreateCalls => Volatile.Read(ref _createCalls);

    public int PublicCalls => Volatile.Read(ref _publicCalls);

    public HttpClient Client() => App.GetTestClient();

    public static async Task<ProbeApi> StartAsync(
        string environment = "Production", Action<WebApplication>? extraRoutes = null, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.AddNibrasTelemetry("Probe");
        builder.Services.AddNibrasWeb("Probe", c => c.Add("PROBE_SESSION_LOCKED", StatusCodes.Status409Conflict, parentSafe: false), ProbeJsonContext.Default);
        builder.Services.AddNibrasValidators(typeof(ProbeApi).Assembly);
        configure?.Invoke(builder);

        var app = builder.Build();
        var probe = new ProbeApi(app);
        app.UseNibrasTelemetry();
        app.UseNibrasWeb();

        // Stands in for the Gateway-resolved tenant, which the identity slices wire.
        app.Use((context, next) =>
        {
            if (Guid.TryParse(context.Request.Headers["X-Nibras-Tenant-Id"], out var tenant))
            {
                context.RequestServices.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
            }

            return next(context);
        });

        var api = app.MapNibrasApi(1);
        api.MapGet("/students/{id:guid}", (Guid id) => NibrasResults.Versioned(Student(id), id, 42u));
        api.MapPost("/students", (CreateStudent body) =>
        {
            Interlocked.Increment(ref probe._createCalls);
            return TypedResults.Created($"/api/v1/probe/students/{StudentId}", Student(StudentId) with { Name = body.Name ?? "" });
        });
        api.MapGet("/students", () => Enumerable.Range(0, 300).Select(_ => Student(StudentId)).ToArray());
        api.MapPost("/attendance-sessions/{id:guid}/lock", (Guid id) =>
            Result.Failure(new Error("PROBE_SESSION_LOCKED", $"Session {id} is locked.")).ToProblem());
        api.MapGet("/problems", (string code) => new NibrasProblemResult(code, "Snapshot of " + code + "."));
        api.MapGet("/uncatalogued-errors", () => Result.Failure(new Error("PROBE_NOT_IN_THE_CATALOG", "A defect.")).ToProblem());
        api.MapGet("/failures", string () => throw new InvalidOperationException("secret connection string"));
        api.MapGet("/public-pages", () =>
        {
            Interlocked.Increment(ref probe._publicCalls);
            return TypedResults.Ok(new[] { Student(StudentId) });
        }).CacheOutput(NibrasOutputCache.Public);
        extraRoutes?.Invoke(app);

        try
        {
            await app.StartAsync().ConfigureAwait(false);
        }
        catch
        {
            await app.DisposeAsync().ConfigureAwait(false);
            throw;
        }

        return probe;
    }

    public static StudentResponse Student(Guid id) => new(
        id,
        "S-0001",
        "Layla",
        EnrolmentStatus.OnLeave,
        new DateTimeOffset(2026, 9, 19, 8, 30, 0, TimeSpan.FromHours(3)),
        new DateOnly(2012, 3, 4),
        TimeSpan.FromMinutes(45),
        null);

    public async ValueTask DisposeAsync()
    {
        await App.StopAsync().ConfigureAwait(false);
        await App.DisposeAsync().ConfigureAwait(false);
    }
}
