using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>TC-PERF-981 (REQ-PERF-031): 304 on a matching If-None-Match, Brotli, output caching of public reads.</summary>
[Trait("TestCase", "TC-PERF-981")]
public sealed class HttpPerformanceTests : IAsyncLifetime
{
    private ProbeApi _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly Uri StudentUri = new($"/api/v1/probe/students/{ProbeApi.StudentId}", UriKind.Relative);

    public async ValueTask InitializeAsync() => _api = await ProbeApi.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    private async Task<HttpResponseMessage> GetAsync(Uri uri, string? ifNoneMatch = null, string? acceptEncoding = null)
    {
        using var client = _api.Client();
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (ifNoneMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", ifNoneMatch);
        }

        if (acceptEncoding is not null)
        {
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(acceptEncoding));
        }

        return await client.SendAsync(request, Ct);
    }

    [Fact]
    public async Task A_single_resource_read_carries_a_strong_etag_from_the_row_version_and_the_id()
    {
        using var response = await GetAsync(StudentUri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var etag = response.Headers.ETag!;
        etag.IsWeak.ShouldBeFalse();
        etag.Tag.ShouldMatch("^\"42-[0-9a-f]{8}\"$");
        etag.Tag.ShouldBe(ETags.For(42, ProbeApi.StudentId));
        ETags.For(42, Guid.CreateVersion7()).ShouldNotBe(etag.Tag);
        ETags.For(43, ProbeApi.StudentId).ShouldNotBe(etag.Tag);
    }

    [Fact]
    public async Task A_matching_if_none_match_gets_304_with_the_same_etag_and_an_empty_body()
    {
        var etag = ETags.For(42, ProbeApi.StudentId);

        using var response = await GetAsync(StudentUri, ifNoneMatch: etag);

        response.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        response.Headers.ETag!.Tag.ShouldBe(etag);
        (await response.Content.ReadAsByteArrayAsync(Ct)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("\"41-00000000\"")]
    [InlineData("W/\"42-00000000\"")]
    public async Task A_stale_or_weak_tag_gets_the_full_resource(string ifNoneMatch)
    {
        using var response = await GetAsync(StudentUri, ifNoneMatch: ifNoneMatch);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)).RootElement.GetProperty("id").GetGuid().ShouldBe(ProbeApi.StudentId);
    }

    [Fact]
    public async Task A_weak_form_of_the_current_tag_does_not_match_either()
    {
        using var response = await GetAsync(StudentUri, ifNoneMatch: "W/" + ETags.For(42, ProbeApi.StudentId));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_client_that_accepts_brotli_gets_a_brotli_body_that_decodes_to_the_same_json()
    {
        using var plain = await GetAsync(new Uri("/api/v1/probe/students", UriKind.Relative));
        using var compressed = await GetAsync(new Uri("/api/v1/probe/students", UriKind.Relative), acceptEncoding: "br");

        compressed.Content.Headers.ContentEncoding.ShouldBe(["br"]);
        var raw = await compressed.Content.ReadAsByteArrayAsync(Ct);
        await using var brotli = new BrotliStream(new MemoryStream(raw), CompressionMode.Decompress);
        using var decoded = new StreamReader(brotli);
        var json = await decoded.ReadToEndAsync(Ct);
        var original = await plain.Content.ReadAsStringAsync(Ct);
        json.ShouldBe(original);
        raw.Length.ShouldBeLessThan(original.Length / 5);
    }

    [Fact]
    public async Task A_public_read_is_served_from_the_output_cache_without_running_the_handler_again()
    {
        var uri = new Uri("/api/v1/probe/public-pages", UriKind.Relative);

        using var first = await GetAsync(uri);
        using var second = await GetAsync(uri);

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await second.Content.ReadAsStringAsync(Ct)).ShouldBe(await first.Content.ReadAsStringAsync(Ct));
        _api.PublicCalls.ShouldBe(1);
    }

    [Fact]
    public async Task A_response_without_its_own_cache_policy_is_no_store()
    {
        using var response = await GetAsync(StudentUri);

        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
    }
}
