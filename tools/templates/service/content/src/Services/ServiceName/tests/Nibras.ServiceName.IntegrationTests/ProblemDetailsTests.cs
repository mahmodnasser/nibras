using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace Nibras.ServiceName.IntegrationTests;

/// <summary>TC-API-031 (REQ-API-009): the service answers an error with Problem Details carrying its catalog code and the correlation id.</summary>
[Trait("TestCase", "TC-API-031")]
public sealed class ProblemDetailsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProblemDetailsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task An_unknown_path_under_the_service_prefix_is_its_not_found_problem()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/servicename/nothing-here", UriKind.Relative), TestContext.Current.CancellationToken);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).RootElement;

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        body.GetProperty("code").GetString().ShouldBe("SERVICENAME_NOT_FOUND");
        body.GetProperty("type").GetString().ShouldBe("urn:nibras:problem:SERVICENAME_NOT_FOUND");
        body.GetProperty("correlationId").GetString().ShouldBe(response.Headers.GetValues("X-Nibras-Correlation-Id").Single());
    }
}
