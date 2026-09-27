using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nibras.ServiceDefaults;
using Shouldly;
using Xunit;

namespace Nibras.ServiceName.IntegrationTests;

/// <summary>TC-INF-964 (REQ-INF-014): liveness, readiness and startup probes, and graceful shutdown.</summary>
[Trait("TestCase", "TC-INF-964")]
public sealed class ProbeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProbeTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Theory]
    [InlineData(ProbePaths.Live)]
    [InlineData(ProbePaths.Ready)]
    [InlineData(ProbePaths.Startup)]
    public async Task A_started_service_answers_every_probe_with_200(string path)
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task An_unreachable_dependency_fails_readiness_and_leaves_liveness_passing()
    {
        using var factory = _factory.WithWebHostBuilder(host => host.ConfigureTestServices(services =>
            services.AddHealthChecks().AddCheck(
                "database",
                () => HealthCheckResult.Unhealthy("The database is unreachable."),
                [ProbeTags.Ready])));
        using var client = factory.CreateClient();

        using var ready = await client.GetAsync(new Uri(ProbePaths.Ready, UriKind.Relative), TestContext.Current.CancellationToken);
        using var live = await client.GetAsync(new Uri(ProbePaths.Live, UriKind.Relative), TestContext.Current.CancellationToken);

        ready.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        live.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_stopping_service_reports_not_ready_so_it_leaves_the_load_balancer_first()
    {
        using var factory = _factory.WithWebHostBuilder(_ => { });
        using var client = factory.CreateClient();
        var health = factory.Services.GetRequiredService<HealthCheckService>();
        var lifetime = factory.Services.GetRequiredService<IHostApplicationLifetime>();
        HealthStatus? whileStopping = null;

        // The load balancer polls readiness while the host drains, so the check runs inside the stopping callback.
        using var registration = lifetime.ApplicationStopping.Register(() =>
            whileStopping = health.CheckHealthAsync(c => c.Tags.Contains(ProbeTags.Ready)).GetAwaiter().GetResult().Status);
        var before = await health.CheckHealthAsync(c => c.Tags.Contains(ProbeTags.Ready), TestContext.Current.CancellationToken);
        lifetime.StopApplication();

        before.Status.ShouldBe(HealthStatus.Healthy);
        whileStopping.ShouldBe(HealthStatus.Unhealthy);
    }

    [Fact]
    public void The_host_waits_for_in_flight_work_before_it_exits()
    {
        var options = _factory.Services.GetRequiredService<IOptions<HostOptions>>().Value;

        options.ShutdownTimeout.ShouldBe(TimeSpan.FromSeconds(30));
    }
}
