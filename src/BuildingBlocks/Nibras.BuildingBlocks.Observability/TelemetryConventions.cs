namespace Nibras.BuildingBlocks.Observability;

/// <summary>
/// The names every service emits under (Appendix L, document 15, document 22 section 1). They are
/// constants so that a dashboard, an alert and a test all read the same string.
/// </summary>
public static class TelemetryConventions
{
    /// <summary>The metric name prefix. Every Nibras metric starts with it (REQ-INF-018).</summary>
    public const string MetricPrefix = "nibras_";

    /// <summary>The tenant label on metrics, spans and log records (REQ-INF-018).</summary>
    public const string TenantAttribute = "nibras.tenant_id";

    /// <summary>The correlation id attribute on spans and log records.</summary>
    public const string CorrelationAttribute = "nibras.correlation_id";

    /// <summary>The correlation header, set by the Gateway if absent and echoed on every response (document 22 section 1).</summary>
    public const string CorrelationHeader = "X-Nibras-Correlation-Id";

    /// <summary>The prefix of every activity source and meter name, so one wildcard subscribes to all of them.</summary>
    public const string InstrumentationPrefix = "Nibras.";

    /// <summary>The instrumentation name of one service, for example <c>Nibras.Attendance</c>.</summary>
    public static string InstrumentationName(string service)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        return InstrumentationPrefix + service;
    }
}
