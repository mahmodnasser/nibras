using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Nibras.BuildingBlocks.Observability;

namespace Nibras.ServiceDefaults;

public static class Extensions
{
    /// <summary>How long a stopping host waits for in-flight requests and messages before it exits.</summary>
    public static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Telemetry, the three probes, graceful shutdown, service discovery and the standard resilience
    /// handler (timeout, retry with backoff and jitter, circuit breaker) on every HttpClient (REQ-INF-014,
    /// REQ-INF-017). Every Api, Worker, Gateway and BFF host calls this once.
    /// </summary>
    public static IHostApplicationBuilder AddNibrasServiceDefaults(this IHostApplicationBuilder builder, string service)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddNibrasTelemetry(service);

        builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = ShutdownTimeout);

        builder.Services.AddSingleton<LifecycleCheck>();
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), [ProbeTags.Live])
            .AddCheck<LifecycleCheck>("lifecycle", tags: [ProbeTags.Ready, ProbeTags.Startup]);

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>Maps the three probes and the telemetry middleware. Probes carry no authentication; they are cluster-network only.</summary>
    public static WebApplication MapNibrasDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        MapProbe(app, ProbePaths.Live, ProbeTags.Live);
        MapProbe(app, ProbePaths.Ready, ProbeTags.Ready);
        MapProbe(app, ProbePaths.Startup, ProbeTags.Startup);
        return app;
    }

    private static void MapProbe(IEndpointRouteBuilder endpoints, string path, string tag) =>
        endpoints.MapHealthChecks(path, new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(tag),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            },
        });
}
