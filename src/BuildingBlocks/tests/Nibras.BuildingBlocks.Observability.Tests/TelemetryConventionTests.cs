using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Observability.Tests;

/// <summary>TC-INF-111: metric and log conventions asserted over the emitted names (REQ-INF-018).</summary>
[Trait("TestCase", "TC-INF-111")]
public sealed class MetricNameTests
{
    [Theory]
    [InlineData("nibras_attendance_marked_total")]
    [InlineData("nibras_outbox_lag_seconds")]
    [InlineData("nibras_x1")]
    public void A_prefixed_lower_snake_case_name_is_accepted(string name)
    {
        using var metrics = new NibrasMetrics("Attendance");
        metrics.Counter(name).Name.ShouldBe(name);
    }

    [Theory]
    [InlineData("attendance_marked_total")]
    [InlineData("nibras_")]
    [InlineData("nibras__double")]
    [InlineData("nibras_trailing_")]
    [InlineData("Nibras_upper")]
    [InlineData("nibras_Attendance")]
    [InlineData("nibras.attendance")]
    [InlineData("nibras-attendance")]
    public void Any_other_name_is_refused_when_the_instrument_is_created(string name)
    {
        using var metrics = new NibrasMetrics("Attendance");
        Should.Throw<ArgumentException>(() => metrics.Histogram(name));
    }

    [Fact]
    public void A_service_meter_and_activity_source_are_named_after_the_service()
    {
        using var metrics = new NibrasMetrics("Attendance");

        metrics.MeterName.ShouldBe("Nibras.Attendance");
        ActivitySources.For("Attendance").Name.ShouldBe("Nibras.Attendance");
    }

    [Fact]
    public void The_tenant_label_is_added_only_when_a_tenant_is_known()
    {
        var tenant = Guid.Parse("018f7c2a-0b1d-7e2f-9c3b-1a2b3c4d5e6f");

        NibrasMetrics.WithTenant(tenant).ShouldContain(new KeyValuePair<string, object?>("nibras.tenant_id", tenant.ToString("D")));
        NibrasMetrics.WithTenant(null).Count.ShouldBe(0);
    }
}

/// <summary>TC-INF-111: the correlation id and the tenant label on a real request pipeline.</summary>
[Trait("TestCase", "TC-INF-111")]
public sealed class CorrelationAndTenantTests
{
    private static readonly Guid TenantA = Guid.Parse("018f7c2a-0b1d-7e2f-9c3b-000000000001");

    private sealed class FixedTenant(Guid? tenant) : ITelemetryTenantSource
    {
        public Guid? CurrentTenantId(HttpContext context) => tenant;
    }

    private static async Task<WebApplication> StartAsync(Guid? tenant, Action<HttpContext>? inspect = null)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.AddNibrasTelemetry("Probe");
        builder.Services.AddSingleton<ITelemetryTenantSource>(new FixedTenant(tenant));
        var app = builder.Build();
        app.UseNibrasTelemetry();
        app.MapGet("/echo", (HttpContext context) =>
        {
            inspect?.Invoke(context);
            return Results.Ok();
        });
        await app.StartAsync(TestContext.Current.CancellationToken);
        return app;
    }

    [Fact]
    public async Task A_well_formed_inbound_correlation_id_is_echoed_unchanged()
    {
        await using var app = await StartAsync(tenant: null);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/echo");
        request.Headers.Add("X-Nibras-Correlation-Id", "018f7c2a-0b1d-7e2f-9c3b-1a2b3c4d5e6f");

        using var response = await app.GetTestClient().SendAsync(request, TestContext.Current.CancellationToken);

        response.Headers.GetValues("X-Nibras-Correlation-Id").Single().ShouldBe("018f7c2a-0b1d-7e2f-9c3b-1a2b3c4d5e6f");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task A_missing_or_malformed_correlation_id_is_replaced_with_a_uuid_v7(string? inbound)
    {
        await using var app = await StartAsync(tenant: null);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/echo");
        if (inbound is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Nibras-Correlation-Id", inbound);
        }

        using var response = await app.GetTestClient().SendAsync(request, TestContext.Current.CancellationToken);

        var echoed = Guid.Parse(response.Headers.GetValues("X-Nibras-Correlation-Id").Single());
        echoed.Version.ShouldBe(7);
        echoed.ToString("D").ShouldNotBe(inbound);
    }

    [Fact]
    public async Task The_request_duration_metric_carries_the_tenant_label_when_the_tenant_is_known()
    {
        // The hosting layer records the duration just after the response is sent, so the test waits for
        // the measurement itself rather than for the response.
        var recorded = new TaskCompletionSource<KeyValuePair<string, object?>[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Name == "http.server.request.duration")
                {
                    l.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<double>((_, _, tags, _) => recorded.TrySetResult(tags.ToArray()));
        listener.Start();

        await using var app = await StartAsync(TenantA);
        using var response = await app.GetTestClient().GetAsync(new Uri("/echo", UriKind.Relative), TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var tags = await recorded.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        tags.ShouldContain(new KeyValuePair<string, object?>("nibras.tenant_id", TenantA.ToString("D")));
    }

    [Fact]
    public async Task The_current_span_carries_the_tenant_and_the_correlation_id()
    {
        string? spanTenant = null;
        string? spanCorrelation = null;
        using var source = new ActivitySource("Nibras.ProbeTest");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        await using var app = await StartAsync(TenantA, _ =>
        {
            spanTenant = Activity.Current?.GetTagItem("nibras.tenant_id") as string;
            spanCorrelation = Activity.Current?.GetTagItem("nibras.correlation_id") as string;
        });
        using var response = await app.GetTestClient().GetAsync(new Uri("/echo", UriKind.Relative), TestContext.Current.CancellationToken);

        spanTenant.ShouldBe(TenantA.ToString("D"));
        spanCorrelation.ShouldBe(response.Headers.GetValues("X-Nibras-Correlation-Id").Single());
    }

    [Fact]
    public async Task No_tenant_label_is_written_when_no_tenant_is_resolved()
    {
        string? spanTenant = "unset";
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        await using var app = await StartAsync(tenant: null, _ => spanTenant = Activity.Current?.GetTagItem("nibras.tenant_id") as string);
        using var response = await app.GetTestClient().GetAsync(new Uri("/echo", UriKind.Relative), TestContext.Current.CancellationToken);

        spanTenant.ShouldBeNull();
    }
}
