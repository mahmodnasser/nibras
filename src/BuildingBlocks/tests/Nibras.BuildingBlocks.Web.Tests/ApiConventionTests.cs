using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>TC-API-951 (REQ-API-001): versioned, service-scoped, kebab-case paths no deeper than one sub-resource.</summary>
[Trait("TestCase", "TC-API-951")]
public sealed class ApiConventionTests
{
    [Theory]
    [InlineData("/api/v1/probe/attendance-sessions")]
    [InlineData("/api/v1/probe/attendance-sessions/{id:guid}")]
    [InlineData("/api/v1/probe/invoices/{id}/lines")]
    [InlineData("/api/v1/probe/invoices/{id}/lines/{lineId}")]
    [InlineData("/api/v1/probe/term-results/{id}/publish")]
    [InlineData("/api/v1/probe/attendance-records/bulk")]
    [InlineData("/api/v2/probe/imports/jobs/{jobId}")]
    [InlineData("/health/ready")]
    public void A_conforming_path_passes(string path) =>
        ApiConventions.Violations([RoutePatternFactory.Parse(path)], "probe").ShouldBeEmpty();

    [Theory]
    [InlineData("/api/v1/probe/gradeLevels")]
    [InlineData("/api/v1/probe/grade_levels")]
    [InlineData("/api/1/probe/students")]
    [InlineData("/api/v0/probe/students")]
    [InlineData("/api/v1/school/students")]
    [InlineData("/api/v1/probe")]
    [InlineData("/api/v1/probe/invoices/{id}/lines/{lineId}/taxes")]
    [InlineData("/api/v1/probe/students/{id}/{other}")]
    [InlineData("/api/v1/probe/students/archive")]
    public void A_breaking_path_is_reported(string path) =>
        ApiConventions.Violations([RoutePatternFactory.Parse(path)], "probe").Count.ShouldBe(1);

    [Fact]
    public async Task The_api_group_is_the_versioned_service_prefix()
    {
        await using var api = await ProbeApi.StartAsync();
        using var client = api.Client();

        using var response = await client.GetAsync(new Uri("/api/v1/probe/students", UriKind.Relative), TestContext.Current.CancellationToken);

        response.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public async Task A_host_with_a_breaking_route_refuses_to_start()
    {
        var failure = await Should.ThrowAsync<InvalidOperationException>(() =>
            ProbeApi.StartAsync(extraRoutes: app => app.MapNibrasApi(1).MapGet("/GradeLevels", () => Results.Ok())));

        failure.Message.ShouldContain("/api/v1/probe/GradeLevels");
    }
}
