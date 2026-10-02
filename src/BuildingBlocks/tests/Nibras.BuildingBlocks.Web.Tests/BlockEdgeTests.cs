using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Nibras.BuildingBlocks.Domain;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>The refusals and edge values of the error catalog and the result mapping (TC-API-031).</summary>
[Trait("TestCase", "TC-API-031")]
public sealed class ErrorCatalogEdgeTests
{
    [Theory]
    [InlineData("ATTENDANCE")]
    [InlineData("attendance_")]
    [InlineData("ATTEN-DANCE_")]
    public void A_prefix_that_is_not_upper_snake_case_with_a_trailing_underscore_is_refused(string prefix) =>
        Should.Throw<ArgumentException>(() => new ErrorCatalog(prefix));

    [Theory]
    [InlineData(399)]
    [InlineData(600)]
    public void A_code_outside_4xx_and_5xx_is_refused(int status) =>
        Should.Throw<ArgumentOutOfRangeException>(() => new ErrorCatalog("PROBE_").Add("PROBE_ODD", status, parentSafe: true));

    [Theory]
    [InlineData(400)]
    [InlineData(599)]
    public void The_first_and_last_error_statuses_are_accepted(int status) =>
        new ErrorCatalog("PROBE_").Add("PROBE_EDGE", status, parentSafe: true).Get("PROBE_EDGE").Status.ShouldBe(status);

    [Fact]
    public void A_code_added_twice_or_never_added_is_a_defect()
    {
        var catalog = new ErrorCatalog("PROBE_").Add("PROBE_SESSION_LOCKED", 409, parentSafe: false);

        Should.Throw<ArgumentException>(() => catalog.Add("PROBE_SESSION_LOCKED", 409, parentSafe: false));
        Should.Throw<ArgumentException>(() => catalog.Add("OTHER_SESSION_LOCKED", 409, parentSafe: false));
        Should.Throw<KeyNotFoundException>(() => catalog.Get("PROBE_NEVER_ADDED"));
        catalog.CrossCutting(ErrorCatalog.NotFound).Status.ShouldBe(404);
        catalog.Get("PROBE_SESSION_LOCKED").Title.ShouldBe("Session locked");
    }

    [Fact]
    public void A_service_name_that_is_not_an_appendix_L_name_is_refused_and_the_options_derive_from_a_valid_one()
    {
        Should.Throw<ArgumentException>(() => new ServiceCollection().AddNibrasWeb("attendance"));
        Should.Throw<ArgumentException>(() => new ServiceCollection().AddNibrasWeb("Atten-dance"));

        var options = new NibrasWebOptions { Service = "Attendance" };
        options.ApiSegment.ShouldBe("attendance");
        options.ErrorPrefix.ShouldBe("ATTENDANCE_");
    }

    [Fact]
    public void A_result_maps_to_204_200_the_callers_result_or_its_problem()
    {
        var locked = new Error("PROBE_SESSION_LOCKED", "Locked.");

        Result.Success().ToHttpResult().ShouldBeOfType<NoContent>();
        Result.Failure(locked).ToHttpResult().ShouldBeOfType<NibrasProblemResult>().Code.ShouldBe("PROBE_SESSION_LOCKED");
        Result.Success(7).ToHttpResult().ShouldBeOfType<Ok<int>>().Value.ShouldBe(7);
        Result.Success(7).ToHttpResult(v => TypedResults.Created("/x", v)).ShouldBeOfType<Created<int>>();
        Result.Failure<int>(locked).ToHttpResult().ShouldBeOfType<NibrasProblemResult>();
        Should.Throw<InvalidOperationException>(() => Result.Success().ToProblem());
    }

    [Fact]
    public void A_bad_request_without_a_json_cause_names_the_whole_body()
    {
        var error = ErrorHandling.FieldErrorOf(new BadHttpRequestException("Bad form."));

        error.Field.ShouldBe("$");
        error.Code.ShouldBe("invalid");
    }

    [Theory]
    [InlineData("NotEmptyValidator", "required")]
    [InlineData("MinimumLengthValidator", "minLength")]
    [InlineData("ExactLengthValidator", "length")]
    [InlineData("EmailValidator", "email")]
    [InlineData("InclusiveBetweenValidator", "outOfRange")]
    [InlineData("RegularExpressionValidator", "pattern")]
    [InlineData("EnumValidator", "unknownEnumValue")]
    [InlineData("PrecisionScaleValidator", "precision")]
    [InlineData("PredicateValidator", "invalid")]
    [InlineData("", "invalid")]
    [InlineData("dateInFuture", "dateInFuture")]
    public void A_validator_code_maps_to_the_shared_validation_catalog(string validator, string expected) =>
        ValidationFilter.CodeOf(validator).ShouldBe(expected);
}

/// <summary>The timestamp and duration converters at their refusals (TC-API-953).</summary>
[Trait("TestCase", "TC-API-953")]
public sealed class ConverterEdgeTests
{
    private static readonly JsonSerializerOptions Options = NibrasJson.Configure(new JsonSerializerOptions());

    [Fact]
    public void A_utc_or_local_datetime_is_written_in_utc_and_an_unspecified_one_is_refused()
    {
        var utc = new DateTime(2026, 9, 19, 5, 30, 0, DateTimeKind.Utc);

        JsonSerializer.Serialize(utc, Options).ShouldBe("\"2026-09-19T05:30:00.000Z\"");
        JsonSerializer.Serialize(utc.ToLocalTime(), Options).ShouldBe("\"2026-09-19T05:30:00.000Z\"");
        Should.Throw<InvalidOperationException>(() => JsonSerializer.Serialize(new DateTime(2026, 9, 19, 5, 30, 0, DateTimeKind.Unspecified), Options));
    }

    [Fact]
    public void A_datetime_is_read_only_with_the_z_suffix()
    {
        JsonSerializer.Deserialize<DateTime>("\"2026-09-19T05:30:00.000Z\"", Options).ShouldBe(new DateTime(2026, 9, 19, 5, 30, 0, DateTimeKind.Utc));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"2026-09-19T05:30:00\"", Options));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<DateTime>("20260919", Options));
    }

    [Theory]
    [InlineData("\"45 minutes\"")]
    [InlineData("null")]
    public void A_duration_that_is_not_an_iso_8601_string_is_refused(string json) =>
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TimeSpan>(json, Options));
}

/// <summary>The parsed <c>If-Match</c> header at its edges (TC-API-020, TC-API-021).</summary>
[Trait("TestCase", "TC-API-020")]
public sealed class IfMatchEdgeTests
{
    private static readonly Guid Id = Guid.Parse("018f6a1e-5c2b-7d3e-9a4f-1b2c3d4e5f60");

    private static IfMatch Parsed(string header)
    {
        IfMatch.Parse(new StringValues(header), out var parsed).ShouldBe(IfMatchProblem.None);
        return parsed;
    }

    [Fact]
    public void Two_tags_never_yield_one_version_but_either_may_match()
    {
        var current = ETags.For(42u, Id);
        var ifMatch = Parsed(ETags.For(41u, Id) + ", " + current);

        ifMatch.TryGetRowVersion(Id, out _).ShouldBeFalse();
        ifMatch.Matches(Id, 42u).ShouldBeTrue();
        ifMatch.Matches(Id, 43u).ShouldBeFalse();
    }

    [Fact]
    public void The_wildcard_matches_any_version_and_cannot_be_combined_with_a_tag()
    {
        Parsed("*").Matches(Id, 7u).ShouldBeTrue();
        IfMatch.Parse(new StringValues("*, " + ETags.For(7u, Id)), out _).ShouldBe(IfMatchProblem.Malformed);
    }

    [Theory]
    [InlineData("\"x-0a1b2c3d\"")]
    [InlineData("\"-0a1b2c3d\"")]
    [InlineData("\"420a1b2c3d\"")]
    public void A_tag_without_a_numeric_version_yields_no_version(string tag) =>
        Parsed(tag).TryGetRowVersion(Id, out _).ShouldBeFalse();

    [Fact]
    public async Task A_handler_parameter_is_bound_even_when_the_header_is_missing()
    {
        var context = new DefaultHttpContext();

        var bound = await IfMatch.BindAsync(context);

        bound.ShouldNotBeNull().IsPresent.ShouldBeFalse();
    }
}
