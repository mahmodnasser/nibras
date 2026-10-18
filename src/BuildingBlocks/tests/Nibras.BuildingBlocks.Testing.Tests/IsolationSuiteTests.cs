using Nibras.BuildingBlocks.Testing.Isolation;
using Nibras.BuildingBlocks.Testing.Isolation.Canary;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Testing.Tests;

/// <summary>TC-SEC-955 (REQ-SEC-005): the suite attacks every endpoint and consumer of a correct service, and every attack fails to cross.</summary>
public sealed class CanaryAttackTests(SuiteFixture<CanaryTarget> canary) : IClassFixture<SuiteFixture<CanaryTarget>>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void The_surfaces_are_every_api_endpoint_once_per_method_and_every_consumer() =>
        canary.Suite.Surfaces.Select(s => s.Name).ShouldBe(
        [
            "DELETE /api/v1/canary/pupils/{id:guid}",
            "GET /api/v1/canary/pupils",
            "GET /api/v1/canary/pupils/{id:guid}",
            "GET /api/v1/canary/sections/{sectionId:guid}/pupils",
            "POST /api/v1/canary/pupils",
            "PUT /api/v1/canary/pupils/{id:guid}",
            "consumer canary.section.closed.v1",
        ]);

    [Fact]
    [Trait("TestCase", "TC-SEC-955")]
    public async Task A_correct_service_resists_every_attack_on_every_surface()
    {
        var outcomes = await canary.Suite.AttackAllAsync(Ct);

        outcomes.Where(o => !o.Passed).ShouldBeEmpty();
        outcomes.Count(o => o.Kind == SurfaceKind.Endpoint).ShouldBe(6);
        outcomes.Count(o => o.Kind == SurfaceKind.Consumer).ShouldBe(1);
        outcomes.Single(o => o.Kind == SurfaceKind.Connection).Attack.ShouldBe(IsolationAttacks.PoolSwap);
    }

    [Theory]
    [InlineData("GET /api/v1/canary/pupils/{id:guid}")]
    [InlineData("PUT /api/v1/canary/pupils/{id:guid}")]
    [InlineData("DELETE /api/v1/canary/pupils/{id:guid}")]
    [InlineData("GET /api/v1/canary/sections/{sectionId:guid}/pupils")]
    [InlineData("POST /api/v1/canary/pupils")]
    public async Task Tenant_B_identifiers_in_the_path_or_the_body_are_answered_404(string surface)
    {
        var outcome = await canary.Suite.AttackAsync(surface, Ct);

        outcome.Attack.ShouldBe(IsolationAttacks.ForeignIdentifier);
        outcome.Detail.ShouldBe("refused tenant B's identifier with 404");
    }

    [Fact]
    [Trait("TestCase", "TC-DATA-645")]
    public async Task A_consumer_given_an_envelope_of_tenant_B_naming_tenant_A_writes_nothing_to_tenant_A()
    {
        var outcome = await canary.Suite.AttackAsync("consumer " + CanaryTarget.SectionClosed, Ct);

        outcome.Passed.ShouldBeTrue(outcome.Detail);
        outcome.Detail.ShouldBe("rejected with InvalidOperationException");
    }
}

/// <summary>The CAP-DATA-01 demo, first half: with the EF tenant filter bypassed, row-level security still stops every attack.</summary>
public sealed class FilterBypassedTests(SuiteFixture<FilterBypassedCanary> canary) : IClassFixture<SuiteFixture<FilterBypassedCanary>>
{
    [Fact]
    public async Task With_the_tenant_filter_bypassed_row_level_security_still_stops_every_attack()
    {
        var outcomes = await canary.Suite.AttackAllAsync(TestContext.Current.CancellationToken);

        outcomes.Where(o => !o.Passed).ShouldBeEmpty();
    }
}

/// <summary>
/// The CAP-DATA-01 demo, second half, and REQ-SEC-005's acceptance: with the filter bypassed and no row-level security,
/// the suite fails, and each failure names the endpoint or consumer that leaked and how.
/// </summary>
public sealed class UnprotectedTests(SuiteFixture<UnprotectedCanary> canary) : IClassFixture<SuiteFixture<UnprotectedCanary>>
{
    [Fact]
    [Trait("TestCase", "TC-SEC-955")]
    public async Task With_both_barriers_gone_every_surface_fails_and_is_named()
    {
        var failed = (await canary.Suite.AttackAllAsync(TestContext.Current.CancellationToken))
            .Where(o => !o.Passed)
            .ToDictionary(o => o.Surface, o => o.Detail);

        failed.Keys.ShouldBe(
            [.. canary.Suite.Surfaces.Select(s => s.Name), IsolationSuite.PoolSwapSurface],
            ignoreOrder: true);
        failed["GET /api/v1/canary/pupils"].ShouldStartWith("answered 200 carrying tenant B's identifier");
        failed["GET /api/v1/canary/sections/{sectionId:guid}/pupils"].ShouldStartWith("answered 200 carrying tenant B's identifier");
        failed["PUT /api/v1/canary/pupils/{id:guid}"].ShouldBe("answered 409 to tenant B's identifier; expected 404, or 403 with _TENANT_MISMATCH");
        failed["DELETE /api/v1/canary/pupils/{id:guid}"].ShouldBe("answered 409 to tenant B's identifier; expected 404, or 403 with _TENANT_MISMATCH");
        failed["POST /api/v1/canary/pupils"].ShouldBe("answered 201 to tenant B's identifier; expected 404, or 403 with _TENANT_MISMATCH");
        failed["consumer " + CanaryTarget.SectionClosed].ShouldBe("an envelope of tenant B naming tenant A's identifier changed tenant A's rows");
    }
}

/// <summary>The pool swap catches a tenant setting that outlives its transaction on a pooled connection (TC-PLAT-015).</summary>
public sealed class SessionLeakTests(SuiteFixture<SessionLeakingCanary> canary) : IClassFixture<SuiteFixture<SessionLeakingCanary>>
{
    [Fact]
    [Trait("TestCase", "TC-PLAT-015")]
    public async Task A_session_level_tenant_setting_fails_the_pool_swap()
    {
        var outcome = await canary.Suite.PoolSwapAsync(TestContext.Current.CancellationToken);

        outcome.Passed.ShouldBeFalse();
        outcome.Detail.ShouldStartWith("a query in no transaction read 6 rows of pupils");
    }
}
