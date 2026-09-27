using System.Net;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>TC-API-031 (REQ-API-009): every error body is Problem Details with the catalog code and the correlation id.</summary>
[Trait("TestCase", "TC-API-031")]
public sealed class ProblemDetailsTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(HttpResponseMessage Response, JsonElement Body)> GetAsync(ProbeApi api, string path, Action<HttpRequestMessage>? configure = null)
    {
        using var client = api.Client();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
        configure?.Invoke(request);
        var response = await client.SendAsync(request, Ct);
        return (response, JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)).RootElement.Clone());
    }

    private static void ShouldBeProblem(HttpResponseMessage response, JsonElement body, string code, int status, string instance)
    {
        ((int)response.StatusCode).ShouldBe(status);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        body.GetProperty("type").GetString().ShouldBe("urn:nibras:problem:" + code);
        body.GetProperty("code").GetString().ShouldBe(code);
        body.GetProperty("status").GetInt32().ShouldBe(status);
        body.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();
        body.GetProperty("detail").GetString().ShouldNotBeNullOrWhiteSpace();
        body.GetProperty("instance").GetString().ShouldBe(instance);
        body.GetProperty("correlationId").GetString().ShouldBe(response.Headers.GetValues("X-Nibras-Correlation-Id").Single());
        body.TryGetProperty("parentSafe", out _).ShouldBeTrue();
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
    }

    [Fact]
    public async Task Every_catalog_code_produces_its_problem_with_the_matching_status_and_correlation_id()
    {
        await using var api = await ProbeApi.StartAsync();
        var catalog = api.App.Services.GetService(typeof(ErrorCatalog)).ShouldBeOfType<ErrorCatalog>();

        catalog.Codes.Count.ShouldBe(9); // seven K.1 error suffixes, INTERNAL_ERROR, and the probe's own
        foreach (var row in catalog.Codes)
        {
            var (response, body) = await GetAsync(api, "/api/v1/probe/problems?code=" + row.Code);

            ShouldBeProblem(response, body, row.Code, row.Status, "/api/v1/probe/problems");
            body.GetProperty("parentSafe").GetBoolean().ShouldBe(row.ParentSafe);
        }
    }

    [Fact]
    public async Task A_failed_result_becomes_its_catalog_problem_with_the_inbound_correlation_id_and_the_tenant()
    {
        await using var api = await ProbeApi.StartAsync();
        var correlation = Guid.CreateVersion7().ToString("D");
        var session = Guid.CreateVersion7();
        using var client = api.Client();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"/api/v1/probe/attendance-sessions/{session}/lock", UriKind.Relative));
        request.Headers.Add("X-Nibras-Correlation-Id", correlation);
        request.Headers.Add("X-Nibras-Tenant-Id", ProbeApi.TenantHeaderValue.ToString("D"));

        using var response = await client.SendAsync(request, Ct);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)).RootElement;

        ShouldBeProblem(response, body, "PROBE_SESSION_LOCKED", 409, $"/api/v1/probe/attendance-sessions/{session}/lock");
        body.GetProperty("correlationId").GetString().ShouldBe(correlation);
        body.GetProperty("tenantId").GetString().ShouldBe(ProbeApi.TenantHeaderValue.ToString("D"));
        body.GetProperty("title").GetString().ShouldBe("Session locked");
        body.GetProperty("parentSafe").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task An_unmatched_route_is_the_services_not_found_problem()
    {
        await using var api = await ProbeApi.StartAsync();

        var (response, body) = await GetAsync(api, "/api/v1/probe/nothing-here?x=1");

        ShouldBeProblem(response, body, "PROBE_NOT_FOUND", 404, "/api/v1/probe/nothing-here");
        body.GetProperty("tenantId").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task An_unhandled_exception_is_internal_error_with_nothing_but_the_reference()
    {
        await using var api = await ProbeApi.StartAsync();

        var (response, body) = await GetAsync(api, "/api/v1/probe/failures");

        ShouldBeProblem(response, body, "PROBE_INTERNAL_ERROR", 500, "/api/v1/probe/failures");
        body.GetProperty("parentSafe").GetBoolean().ShouldBeFalse();
        body.GetProperty("detail").GetString().ShouldBe($"Reference {body.GetProperty("correlationId").GetString()}.");
        body.ToString().ShouldNotContain("secret connection string");
        body.TryGetProperty("traceId", out _).ShouldBeFalse(); // production
    }

    [Fact]
    public async Task A_code_missing_from_the_catalog_is_a_defect_that_surfaces_as_internal_error()
    {
        await using var api = await ProbeApi.StartAsync();

        var (response, body) = await GetAsync(api, "/api/v1/probe/uncatalogued-errors");

        ShouldBeProblem(response, body, "PROBE_INTERNAL_ERROR", 500, "/api/v1/probe/uncatalogued-errors");
    }

    [Fact]
    public async Task Outside_production_the_body_carries_the_trace_id()
    {
        await using var api = await ProbeApi.StartAsync("Development");

        var (_, body) = await GetAsync(api, "/api/v1/probe/failures");

        body.GetProperty("traceId").GetString()!.ShouldMatch("^[0-9a-f]{32}$");
    }

    [Fact]
    public void A_catalog_refuses_a_code_without_its_prefix_or_twice()
    {
        var catalog = new ErrorCatalog("PROBE_");

        Should.Throw<ArgumentException>(() => catalog.Add("OTHER_THING", 409, parentSafe: false));
        Should.Throw<ArgumentException>(() => catalog.Add("PROBE_NOT_FOUND", 404, parentSafe: true));
        Should.Throw<KeyNotFoundException>(() => catalog.Get("PROBE_UNKNOWN"));
    }
}
