using Nibras.BuildingBlocks.Testing.Isolation;
using Xunit;

[assembly: AssemblyFixture(typeof(Nibras.TenantIsolation.Tests.IsolationRun))]

namespace Nibras.TenantIsolation.Tests;

/// <summary>
/// The verdicts of one run, written when the run ends to the file <c>NIBRAS_ISOLATION_REPORT</c> names. <c>tools/ci/ci.mjs</c>
/// reads it for the phase-gate evidence and for the Severity 1 incident record of every failure (REQ-SEC-020).
/// </summary>
public sealed class IsolationRun : IAsyncDisposable
{
    public static IsolationReport Report { get; } = new();

    public async ValueTask DisposeAsync()
    {
        if (Environment.GetEnvironmentVariable(IsolationReport.PathVariable) is { Length: > 0 } path)
        {
            await Report.WriteAsync(path, CancellationToken.None);
        }
    }
}

/// <summary>One service's suite for its generated class: started once, attacked surface by surface, every verdict recorded.</summary>
public sealed class IsolationFixture<TTarget> : IAsyncLifetime
    where TTarget : IsolationTarget, new()
{
    private IsolationSuite? _suite;

    public async ValueTask InitializeAsync() => _suite = await IsolationSuite.StartAsync(new TTarget(), TestContext.Current.CancellationToken);

    public Task AttackAsync(string surface) => VerdictAsync(suite => suite.AttackAsync(surface, TestContext.Current.CancellationToken));

    public Task PoolSwapAsync() => VerdictAsync(suite => suite.PoolSwapAsync(TestContext.Current.CancellationToken));

    public async ValueTask DisposeAsync()
    {
        if (_suite is not null)
        {
            await _suite.DisposeAsync();
        }
    }

    private async Task VerdictAsync(Func<IsolationSuite, Task<AttackOutcome>> attack)
    {
        var outcome = await attack(_suite!);
        IsolationRun.Report.Record(outcome);
        Assert.True(
            outcome.Passed,
            $"{outcome.Service} {outcome.Surface}, {outcome.Attack}: {outcome.Detail}. A cross-tenant exposure is Severity 1 even when a test catches it (master brief Section 31).");
    }
}
