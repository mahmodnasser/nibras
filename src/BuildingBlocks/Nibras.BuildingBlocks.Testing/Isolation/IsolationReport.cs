using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nibras.BuildingBlocks.Testing.Isolation;

/// <summary>
/// What one run of the suite attacked and what failed: the evidence line of the Appendix V phase gate (the count of
/// endpoints and consumers attacked, document 12 part 4.4) and the input of the Severity 1 incident record that
/// <c>tools/ci/ci.mjs</c> writes for every failure (REQ-SEC-020).
/// </summary>
public sealed class IsolationReport
{
    /// <summary>The environment variable naming the file the suite writes its report to.</summary>
    public const string PathVariable = "NIBRAS_ISOLATION_REPORT";

    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    private readonly ConcurrentQueue<AttackOutcome> _outcomes = new();

    public void Record(AttackOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        _outcomes.Enqueue(outcome);
    }

    public IReadOnlyList<AttackOutcome> Outcomes => [.. _outcomes];

    public string ToJson()
    {
        // One verdict per surface: a surface attacked twice in a run counts once, and fails if either attack failed.
        var verdicts = Outcomes
            .GroupBy(o => (o.Service, o.Surface))
            .Select(g => g.FirstOrDefault(o => !o.Passed) ?? g.First())
            .OrderBy(o => o.Service, StringComparer.Ordinal)
            .ThenBy(o => o.Surface, StringComparer.Ordinal)
            .ToList();
        var report = new JsonObject
        {
            ["endpointsAttacked"] = verdicts.Count(o => o.Kind == SurfaceKind.Endpoint),
            ["consumersAttacked"] = verdicts.Count(o => o.Kind == SurfaceKind.Consumer),
            ["poolSwaps"] = verdicts.Count(o => o.Kind == SurfaceKind.Connection),
            ["services"] = new JsonArray([.. verdicts.GroupBy(o => o.Service).Select(g => (JsonNode)new JsonObject
            {
                ["service"] = g.Key,
                ["endpoints"] = g.Count(o => o.Kind == SurfaceKind.Endpoint),
                ["consumers"] = g.Count(o => o.Kind == SurfaceKind.Consumer),
                ["failed"] = g.Count(o => !o.Passed),
            })]),
            ["failures"] = new JsonArray([.. verdicts.Where(o => !o.Passed).Select(o => (JsonNode)new JsonObject
            {
                ["service"] = o.Service,
                ["surface"] = o.Surface,
                ["attack"] = o.Attack,
                ["detail"] = o.Detail,
            })]),
        };
        return report.ToJsonString(Indented);
    }

    public async Task WriteAsync(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, ToJson() + "\n", cancellationToken).ConfigureAwait(false);
    }
}
