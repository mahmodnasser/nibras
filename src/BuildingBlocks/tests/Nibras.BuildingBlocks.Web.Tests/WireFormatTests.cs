using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>The JSON rules of document 22 §1.4, through a real host with a source-generated context.</summary>
public sealed class WireFormatTests : IAsyncLifetime
{
    private ProbeApi _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => _api = await ProbeApi.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    private async Task<(HttpResponseMessage Response, JsonElement Body)> PostAsync(string json)
    {
        using var client = _api.Client();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(new Uri("/api/v1/probe/students", UriKind.Relative), content, Ct);
        var text = await response.Content.ReadAsStringAsync(Ct);
        return (response, JsonDocument.Parse(text).RootElement.Clone());
    }

    private const string ValidBody = """{"studentNumber":"S-0001","name":"Layla","dateOfBirth":"2012-03-04","status":"enrolled"}""";

    [Fact]
    [Trait("TestCase", "TC-API-952")]
    public async Task Properties_are_camelCase_enums_are_camelCase_strings_and_nulls_are_omitted()
    {
        using var client = _api.Client();

        var text = await client.GetStringAsync(new Uri($"/api/v1/probe/students/{ProbeApi.StudentId}", UriKind.Relative), Ct);
        var body = JsonDocument.Parse(text).RootElement;

        body.GetProperty("studentNumber").GetString().ShouldBe("S-0001");
        body.GetProperty("status").GetString().ShouldBe("onLeave");
        body.TryGetProperty("middleName", out _).ShouldBeFalse();
        body.TryGetProperty("StudentNumber", out _).ShouldBeFalse();
    }

    [Fact]
    [Trait("TestCase", "TC-API-952")]
    public async Task An_enum_sent_as_an_integer_is_refused_naming_the_field()
    {
        var (response, body) = await PostAsync("""{"studentNumber":"S-0001","dateOfBirth":"2012-03-04","status":1}""");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        body.GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("status");
        _api.CreateCalls.ShouldBe(0);
    }

    [Fact]
    [Trait("TestCase", "TC-API-953")]
    public async Task Timestamps_are_utc_with_milliseconds_and_Z_dates_have_ten_characters_and_durations_are_iso()
    {
        using var client = _api.Client();

        var text = await client.GetStringAsync(new Uri($"/api/v1/probe/students/{ProbeApi.StudentId}", UriKind.Relative), Ct);
        var body = JsonDocument.Parse(text).RootElement;

        body.GetProperty("createdAt").GetString().ShouldBe("2026-09-19T05:30:00.000Z"); // 08:30 at +03:00
        body.GetProperty("dateOfBirth").GetString().ShouldBe("2012-03-04");
        body.GetProperty("dateOfBirth").GetString()!.Length.ShouldBe(10);
        body.GetProperty("lessonLength").GetString().ShouldBe("PT45M");
    }

    [Fact]
    [Trait("TestCase", "TC-API-953")]
    public async Task A_timestamp_sent_without_Z_is_refused_rather_than_guessed()
    {
        var (response, body) = await PostAsync(
            """{"studentNumber":"S-0001","dateOfBirth":"2012-03-04","status":"enrolled","enrolledAt":"2026-09-19T08:30:00+03:00"}""");
        var (accepted, _) = await PostAsync(
            """{"studentNumber":"S-0001","dateOfBirth":"2012-03-04","status":"enrolled","enrolledAt":"2026-09-19T05:30:00.000Z"}""");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("enrolledAt");
        accepted.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    [Trait("TestCase", "TC-API-954")]
    public async Task An_unknown_property_is_refused_with_one_error_naming_it()
    {
        var (response, body) = await PostAsync(
            """{"studnetNumber":"S-0001","studentNumber":"S-0001","dateOfBirth":"2012-03-04","status":"enrolled"}""");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        body.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        var errors = body.GetProperty("errors");
        errors.GetArrayLength().ShouldBe(1);
        errors[0].GetProperty("field").GetString().ShouldBe("studnetNumber");
        errors[0].GetProperty("code").GetString().ShouldBe("unknownProperty");
        body.GetProperty("detail").GetString()!.ShouldNotContain("CreateStudent"); // no .NET type names leak
        _api.CreateCalls.ShouldBe(0);
    }

    [Fact]
    [Trait("TestCase", "TC-API-954")]
    public async Task The_same_body_without_the_unknown_property_is_accepted()
    {
        var (response, body) = await PostAsync(ValidBody);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        body.GetProperty("id").GetString().ShouldBe(ProbeApi.StudentId.ToString("D"));
        _api.CreateCalls.ShouldBe(1);
    }
}
