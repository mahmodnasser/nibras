using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Nibras.BuildingBlocks.Observability;

/// <summary>
/// Creates a service's instruments and refuses any name that does not follow the convention:
/// lower snake case with the <c>nibras_</c> prefix (REQ-INF-018, TC-INF-111).
/// </summary>
public sealed class NibrasMetrics : IDisposable
{
    private readonly Meter _meter;

    public NibrasMetrics(string service) => _meter = new Meter(TelemetryConventions.InstrumentationName(service));

    public string MeterName => _meter.Name;

    public Counter<long> Counter(string name, string? unit = null, string? description = null) =>
        _meter.CreateCounter<long>(Checked(name), unit, description);

    public Histogram<double> Histogram(string name, string? unit = null, string? description = null) =>
        _meter.CreateHistogram<double>(Checked(name), unit, description);

    public void Dispose() => _meter.Dispose();

    /// <summary>True when the name is lower snake case and starts with <c>nibras_</c>.</summary>
    public static bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || !name.StartsWith(TelemetryConventions.MetricPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var previousWasUnderscore = false;
        for (var i = TelemetryConventions.MetricPrefix.Length; i < name.Length; i++)
        {
            var c = name[i];
            if (c == '_')
            {
                if (previousWasUnderscore || i == TelemetryConventions.MetricPrefix.Length)
                {
                    return false;
                }

                previousWasUnderscore = true;
                continue;
            }

            if (c is not ((>= 'a' and <= 'z') or (>= '0' and <= '9')))
            {
                return false;
            }

            previousWasUnderscore = false;
        }

        return name.Length > TelemetryConventions.MetricPrefix.Length && !previousWasUnderscore;
    }

    /// <summary>Adds the tenant label to a set of tags when a tenant is known.</summary>
    public static TagList WithTenant(Guid? tenantId, TagList tags = default)
    {
        if (tenantId is { } id)
        {
            tags.Add(TelemetryConventions.TenantAttribute, id.ToString("D"));
        }

        return tags;
    }

    private static string Checked(string name) =>
        IsValidName(name)
            ? name
            : throw new ArgumentException(
                $"Metric name '{name}' must be lower snake case and start with '{TelemetryConventions.MetricPrefix}'.", nameof(name));
}

/// <summary>One activity source per service, named <c>Nibras.&lt;Service&gt;</c>.</summary>
public static class ActivitySources
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ActivitySource> Sources =
        new(StringComparer.Ordinal);

    public static ActivitySource For(string service) =>
        Sources.GetOrAdd(TelemetryConventions.InstrumentationName(service), static name => new ActivitySource(name));
}
