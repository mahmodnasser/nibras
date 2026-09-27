using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Nibras.BuildingBlocks.Observability;

/// <summary>
/// Supplies the tenant of the current request to telemetry. The Tenancy block registers the real
/// source once it resolves tenants (SL-DATA-001); until then no tenant label is written, never a guessed one.
/// </summary>
public interface ITelemetryTenantSource
{
    Guid? CurrentTenantId(HttpContext context);
}

internal sealed class NoTelemetryTenantSource : ITelemetryTenantSource
{
    public Guid? CurrentTenantId(HttpContext context) => null;
}

/// <summary>
/// Accepts or creates the correlation id, echoes it on the response, and puts the correlation id and
/// the tenant on the current span, the request-duration metric and every log record of the request.
/// </summary>
public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ITelemetryTenantSource tenantSource,
    TimeProvider timeProvider,
    ILogger<CorrelationIdMiddleware> logger)
{
    public const string ItemKey = "nibras.correlation_id";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ReadOrCreate(context.Request.Headers[TelemetryConventions.CorrelationHeader], timeProvider);
        var correlation = correlationId.ToString("D");
        context.Items[ItemKey] = correlation;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[TelemetryConventions.CorrelationHeader] = correlation;
            return Task.CompletedTask;
        });

        var tenant = tenantSource.CurrentTenantId(context)?.ToString("D");
        var activity = Activity.Current;
        activity?.SetTag(TelemetryConventions.CorrelationAttribute, correlation);
        if (tenant is not null)
        {
            activity?.SetTag(TelemetryConventions.TenantAttribute, tenant);
            context.Features.Get<IHttpMetricsTagsFeature>()?.Tags.Add(new(TelemetryConventions.TenantAttribute, tenant));
        }

        var scope = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [TelemetryConventions.CorrelationAttribute] = correlation,
            [TelemetryConventions.TenantAttribute] = tenant,
        };
        using (logger.BeginScope(scope))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    /// <summary>A well-formed inbound id is kept so a request can be followed across services; anything else is replaced.</summary>
    internal static Guid ReadOrCreate(string? inbound, TimeProvider timeProvider) =>
        Guid.TryParseExact(inbound, "D", out var parsed) && parsed != Guid.Empty
            ? parsed
            : Guid.CreateVersion7(timeProvider.GetUtcNow());
}
