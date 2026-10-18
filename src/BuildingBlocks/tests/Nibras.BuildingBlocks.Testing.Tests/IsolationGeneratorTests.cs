using System.Text.Json.Nodes;
using Nibras.BuildingBlocks.Testing.Isolation;
using Nibras.BuildingBlocks.Testing.Isolation.Canary;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Testing.Tests;

public sealed class IsolationGeneratorTests
{
    private static readonly string[] Surfaces = ["GET /api/v1/canary/pupils/{id:guid}", "consumer canary.section.closed.v1"];

    [Theory]
    [InlineData("GET /api/v1/attendance/sessions/{id:guid}", "GetSessionsById_AsTenantA_LeaksNothingOfTenantB")]
    [InlineData("POST /api/v1/attendance/attendance-sessions/{sessionId}/lock", "PostAttendanceSessionsBySessionIdLock_AsTenantA_LeaksNothingOfTenantB")]
    [InlineData("GET /api/v1/canary/pupils", "GetPupils_AsTenantA_LeaksNothingOfTenantB")]
    [InlineData("consumer attendance.session.closed.v1", "ConsumeAttendanceSessionClosedV1_EnvelopeOfTenantB_WritesNothingToTenantA")]
    public void A_surface_becomes_a_method_name_that_states_the_behaviour(string surface, string expected) =>
        IsolationGenerator.MethodName(surface).ShouldBe(expected);

    [Fact]
    public void The_class_lists_every_surface_and_has_one_test_per_surface_and_the_pool_swap()
    {
        var code = IsolationGenerator.Emit("Canary", "Targets.Canary", Surfaces);

        code.ShouldContain("public sealed class CanaryIsolation(IsolationFixture<Targets.Canary> suite) : IClassFixture<IsolationFixture<Targets.Canary>>");
        code.ShouldContain("        \"GET /api/v1/canary/pupils/{id:guid}\",\n        \"consumer canary.section.closed.v1\",\n    ];");
        code.ShouldContain("    public Task GetPupilsById_AsTenantA_LeaksNothingOfTenantB() => suite.AttackAsync(\"GET /api/v1/canary/pupils/{id:guid}\");");
        code.ShouldContain("    [Trait(\"TestCase\", \"TC-DATA-645\")]\n    public Task ConsumeCanarySectionClosedV1_EnvelopeOfTenantB_WritesNothingToTenantA()");
        code.ShouldContain("public Task PooledConnection_AfterTenantA_ServesTenantBNothingOfTenantA() => suite.PoolSwapAsync();");
        code.Split("[Fact]").Length.ShouldBe(Surfaces.Length + 2);
        code.ShouldNotContain("\r");
    }

    [Fact]
    public void Emitting_twice_gives_the_same_text_so_the_diff_in_CI_shows_only_real_change() =>
        IsolationGenerator.Emit("Canary", "Targets.Canary", Surfaces).ShouldBe(IsolationGenerator.Emit("Canary", "Targets.Canary", Surfaces));

    [Fact]
    public void Two_surfaces_with_the_same_method_name_get_distinct_names()
    {
        var code = IsolationGenerator.Emit("Canary", "Targets.Canary", ["GET /api/v1/canary/pupils/{id}", "GET /api/v1/canary/pupils/{id:guid}"]);

        code.ShouldContain("GetPupilsById_AsTenantA_LeaksNothingOfTenantB()");
        code.ShouldContain("GetPupilsById_AsTenantA_LeaksNothingOfTenantB_2()");
    }
}

public sealed class IsolationReportTests
{
    [Fact]
    public void The_report_counts_each_surface_once_and_a_failed_attack_wins()
    {
        var report = new IsolationReport();
        report.Record(new AttackOutcome("Canary", "GET /api/v1/canary/pupils", SurfaceKind.Endpoint, IsolationAttacks.ForeignRows, true, "answered 200"));
        report.Record(new AttackOutcome("Canary", "GET /api/v1/canary/pupils", SurfaceKind.Endpoint, IsolationAttacks.ForeignRows, false, "leaked"));
        report.Record(new AttackOutcome("Canary", "consumer canary.section.closed.v1", SurfaceKind.Consumer, IsolationAttacks.ConsumerEnvelope, true, "rejected"));
        report.Record(new AttackOutcome("Canary", IsolationSuite.PoolSwapSurface, SurfaceKind.Connection, IsolationAttacks.PoolSwap, true, "ok"));

        var json = JsonNode.Parse(report.ToJson())!;

        json["endpointsAttacked"]!.GetValue<int>().ShouldBe(1);
        json["consumersAttacked"]!.GetValue<int>().ShouldBe(1);
        json["poolSwaps"]!.GetValue<int>().ShouldBe(1);
        json["services"]![0]!["failed"]!.GetValue<int>().ShouldBe(1);
        json["failures"]!.AsArray().Count.ShouldBe(1);
        json["failures"]![0]!["surface"]!.GetValue<string>().ShouldBe("GET /api/v1/canary/pupils");
        json["failures"]![0]!["detail"]!.GetValue<string>().ShouldBe("leaked");
    }
}

public sealed class RequestBodiesTests
{
    [Fact]
    public void A_body_is_well_formed_and_carries_tenant_B_in_every_identifier_it_can_resolve()
    {
        var foreign = Guid.Parse("018f0000-0000-7000-8000-0000000000bb");

        var body = RequestBodies.Sample(typeof(CreateCanaryPupil), name => name == "sectionId" ? foreign : null);

        body.ToJsonString().ShouldBe($$"""{"name":"isolation","sectionId":"{{foreign:D}}"}""");
    }

    [Theory]
    [InlineData("sectionId", "sections")]
    [InlineData("classId", "classes")]
    [InlineData("categoryId", "categories")]
    [InlineData("homeroomTeacherId", "homeroom-teachers")]
    public void An_identifier_property_names_its_resource(string property, string resource) =>
        ResourceNames.ResourcesFor(property).ShouldContain(resource);
}
