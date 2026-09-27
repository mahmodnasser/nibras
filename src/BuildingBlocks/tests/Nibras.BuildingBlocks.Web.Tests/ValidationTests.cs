using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation.Results;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>TC-SEC-967 (REQ-SEC-017): input validated by FluentValidation into Problem Details, output encoded.</summary>
[Trait("TestCase", "TC-SEC-967")]
public sealed class ValidationTests : IAsyncLifetime
{
    private ProbeApi _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => _api = await ProbeApi.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    private async Task<(HttpResponseMessage Response, string Text)> PostAsync(string json)
    {
        using var client = _api.Client();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(new Uri("/api/v1/probe/students", UriKind.Relative), content, Ct);
        return (response, await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task An_empty_required_field_is_refused_before_the_handler_runs()
    {
        var (response, text) = await PostAsync("""{"studentNumber":"","dateOfBirth":"2012-03-04","status":"enrolled"}""");
        var body = JsonDocument.Parse(text).RootElement;

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        body.GetProperty("parentSafe").GetBoolean().ShouldBeTrue();
        var error = body.GetProperty("errors").EnumerateArray().Single();
        error.GetProperty("field").GetString().ShouldBe("studentNumber");
        error.GetProperty("code").GetString().ShouldBe("required");
        _api.CreateCalls.ShouldBe(0);
    }

    [Theory]
    [InlineData(11, HttpStatusCode.Created)]
    [InlineData(12, HttpStatusCode.Created)]
    [InlineData(13, HttpStatusCode.BadRequest)]
    public async Task The_maximum_length_holds_at_the_value_one_below_and_one_above(int length, HttpStatusCode expected)
    {
        var (response, text) = await PostAsync($$"""{"studentNumber":"{{new string('7', length)}}","dateOfBirth":"2012-03-04","status":"enrolled"}""");

        response.StatusCode.ShouldBe(expected);
        if (expected == HttpStatusCode.BadRequest)
        {
            var error = JsonDocument.Parse(text).RootElement.GetProperty("errors")[0];
            error.GetProperty("code").GetString().ShouldBe("maxLength");
            error.GetProperty("params").GetProperty("maxLength").GetInt32().ShouldBe(12);
            text.ShouldNotContain("7777777777777"); // the submitted value is never echoed
        }
    }

    [Fact]
    public async Task A_script_tag_is_stored_as_text_and_comes_back_encoded()
    {
        var (response, text) = await PostAsync("""{"studentNumber":"S-1","name":"<script>alert(1)</script>","dateOfBirth":"2012-03-04","status":"enrolled"}""");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        text.ShouldNotContain("<script>");
        text.ShouldContain("\\u003Cscript\\u003E");
        JsonDocument.Parse(text).RootElement.GetProperty("name").GetString().ShouldBe("<script>alert(1)</script>");
    }

    [Fact]
    public async Task Arabic_text_travels_unescaped()
    {
        var (response, text) = await PostAsync("{\"studentNumber\":\"S-1\",\"name\":\"ليلى\",\"dateOfBirth\":\"2012-03-04\",\"status\":\"enrolled\"}");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        text.ShouldContain("\"name\":\"ليلى\"");
    }

    [Theory]
    [InlineData("Items[2].Amount.Currency", "items[2].amount.currency")]
    [InlineData("DateOfBirth", "dateOfBirth")]
    [InlineData("", "$")]
    public void A_field_is_named_by_the_json_path_the_client_sent(string propertyName, string expected) =>
        ValidationFilter.FieldPath(propertyName).ShouldBe(expected);

    [Theory]
    [InlineData("NotNullValidator", "required")]
    [InlineData("InclusiveBetweenValidator", "outOfRange")]
    [InlineData("EnumValidator", "unknownEnumValue")]
    [InlineData("SomeFutureValidator", "invalid")]
    [InlineData("dateInFuture", "dateInFuture")]
    public void A_validator_code_maps_to_the_shared_validation_catalog(string errorCode, string expected) =>
        ValidationFilter.ToFieldError(new ValidationFailure("DateOfBirth", "m") { ErrorCode = errorCode }).Code.ShouldBe(expected);
}
