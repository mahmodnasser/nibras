using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Nibras.ServiceDefaults;

/// <summary>
/// The probe tags. A check tagged <see cref="Live"/> answers "is the process able to work at all";
/// <see cref="Ready"/> answers "may it take traffic now", which is where database and broker checks go;
/// <see cref="Startup"/> answers "has it finished starting". A failed dependency fails readiness and
/// never liveness, so the replica leaves the load balancer without being restarted (REQ-INF-014).
/// </summary>
public static class ProbeTags
{
    public const string Live = "live";
    public const string Ready = "ready";
    public const string Startup = "startup";
}

/// <summary>The probe paths, the same on every service (document 22's endpoint tables).</summary>
public static class ProbePaths
{
    public const string Live = "/health/live";
    public const string Ready = "/health/ready";
    public const string Startup = "/health/startup";
}

/// <summary>
/// Healthy once the host has started and until it begins to stop. It backs the startup probe, and the
/// readiness probe too, so a stopping replica leaves the load balancer before in-flight work is cut.
/// </summary>
internal sealed class LifecycleCheck(IHostApplicationLifetime lifetime) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(
            lifetime.ApplicationStopping.IsCancellationRequested ? HealthCheckResult.Unhealthy("The service is shutting down.")
            : lifetime.ApplicationStarted.IsCancellationRequested ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("The service is still starting."));
}
