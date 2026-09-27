using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nibras.BuildingBlocks.Observability;

public static class DependencyInjection
{
    /// <summary>
    /// OpenTelemetry traces, metrics and logs for one service (REQ-INF-017), exported over OTLP when
    /// <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is set, plus the service's <see cref="NibrasMetrics"/>.
    /// </summary>
    public static IHostApplicationBuilder AddNibrasTelemetry(this IHostApplicationBuilder builder, string service)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(service);

        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<ITelemetryTenantSource, NoTelemetryTenantSource>();
        builder.Services.TryAddSingleton(_ => new NibrasMetrics(service));

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otel = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: "nibras-" + service.ToLowerInvariant(),
                serviceNamespace: "nibras"))
            .WithTracing(tracing => tracing
                .AddSource(TelemetryConventions.InstrumentationPrefix + "*")
                .AddAspNetCoreInstrumentation(options =>
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health", StringComparison.Ordinal))
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddMeter(TelemetryConventions.InstrumentationPrefix + "*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation());

        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            otel.UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>Adds the correlation and tenant enrichment. Call it first, before routing.</summary>
    public static IApplicationBuilder UseNibrasTelemetry(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
