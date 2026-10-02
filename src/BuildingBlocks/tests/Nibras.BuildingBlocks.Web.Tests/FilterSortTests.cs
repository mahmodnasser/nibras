using System.Net;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>TC-API-010 (REQ-API-008): each operator of document 22 §3.1 returns exactly the expected rows of PostgreSQL.</summary>
[Collection(RosterDefinition.Name)]
[Trait("TestCase", "TC-API-010")]
public sealed class FilterSortTests(RosterFixture roster)
{
    private const string Pupils = "/api/v1/probe/pupils";

    private static readonly DateTimeOffset SixUtc = new(2026, 9, 1, 6, 0, 0, TimeSpan.Zero);

    public static TheoryData<string, string> Operators => new()
    {
        { "eq", "filter[lastName]=family007" },
        { "eq-explicit", "filter[lastName][eq]=family007" },
        { "ne", "filter[status][ne]=withdrawn" },
        { "gt", "filter[balance][gt]=250.25" },
        { "gte", "filter[dateOfBirth][gte]=2014-12-01" },
        { "lt", "filter[enrolledAt][lt]=2026-09-01T06:00:00Z" },
        { "lte", "filter[balance][lte]=10.25" },
        { "in", "filter[status][in]=enrolled,suspended" },
        { "nin", "filter[lastName][nin]=family000,family001" },
        { "contains", "filter[nickname][contains]=ICK1" },
        { "startsWith", "filter[firstName][startsWith]=GIVEN0" },
        { "isNull", "filter[sectionId][isNull]=true" },
        { "isNotNull", "filter[nickname][isNull]=false" },
        { "between", "filter[dateOfBirth][between]=2014-01-10..2014-01-20" },
        { "combined", "filter[status]=enrolled&filter[sectionId]=018f1111-0000-7000-8000-000000000001&filter[balance][gte]=100" },
        { "escaped-comma", "filter[lastName][in]=family001%2Cx,family002" },
        { "literal-percent", "filter[lastName][contains]=%25" },
    };

    private static bool Expected(string name, RosterPupil p) => name switch
    {
        "eq" or "eq-explicit" => p.LastName == "family007",
        "ne" => p.Status != PupilStatus.Withdrawn,
        "gt" => p.Balance > 250.25m,
        "gte" => p.DateOfBirth >= new DateOnly(2014, 12, 1),
        "lt" => p.EnrolledAt < SixUtc,
        "lte" => p.Balance <= 10.25m,
        "in" => p.Status is PupilStatus.Enrolled or PupilStatus.Suspended,
        "nin" => p.LastName is not ("family000" or "family001"),
        "contains" => p.Nickname?.Contains("ick1", StringComparison.OrdinalIgnoreCase) == true,
        "startsWith" => p.FirstName.StartsWith("given0", StringComparison.OrdinalIgnoreCase),
        "isNull" => p.SectionId is null,
        "isNotNull" => p.Nickname is not null,
        "between" => p.DateOfBirth >= new DateOnly(2014, 1, 10) && p.DateOfBirth <= new DateOnly(2014, 1, 20),
        "combined" => p.Status == PupilStatus.Enrolled && p.SectionId == RosterFixture.Sections[0] && p.Balance >= 100m,
        "escaped-comma" => p.LastName == "family002",
        "literal-percent" => false,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };

    [Theory]
    [MemberData(nameof(Operators))]
    public async Task Each_operator_returns_exactly_the_expected_rows(string name, string filter)
    {
        var pages = await RosterClient.AllPagesAsync(roster, Pupils + "?pageSize=200&" + filter);

        var actual = pages.SelectMany(RosterClient.Ids).ToList();
        var expected = roster.Seeded.Where(p => Expected(name, p)).Select(p => p.Id).ToHashSet();
        actual.Count.ShouldBe(expected.Count, name);
        actual.ToHashSet().SetEquals(expected).ShouldBeTrue(name);
        if (name != "literal-percent")
        {
            expected.Count.ShouldBeGreaterThan(0, "a predicate that matches nothing proves nothing");
        }
    }

    [Fact]
    public async Task Between_is_inclusive_at_both_ends()
    {
        var pages = await RosterClient.AllPagesAsync(roster, Pupils + "?pageSize=200&filter[dateOfBirth][between]=2014-01-10..2014-01-20&sort=dateOfBirth");

        var births = pages.SelectMany(p => p.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("dateOfBirth").GetString())).ToList();
        births.ShouldContain("2014-01-10");
        births.ShouldContain("2014-01-20");
        births.ShouldNotContain("2014-01-09");
        births.ShouldNotContain("2014-01-21");
    }

    [Fact]
    public async Task The_sort_is_echoed_with_explicit_signs_and_the_id_appended()
    {
        var (status, page) = await RosterClient.GetAsync(roster, Pupils + "?sort=lastName");

        status.ShouldBe(HttpStatusCode.OK);
        page.GetProperty("sort").GetString().ShouldBe("+lastName,+id");
    }

    [Fact]
    public async Task A_filter_changes_the_filter_hash_and_the_same_filter_in_another_order_does_not()
    {
        var (_, none) = await RosterClient.GetAsync(roster, Pupils);
        var (_, ab) = await RosterClient.GetAsync(roster, Pupils + "?filter[status]=enrolled&filter[balance][gt]=1");
        var (_, ba) = await RosterClient.GetAsync(roster, Pupils + "?filter[balance][gt]=1&filter[status]=enrolled");

        ab.GetProperty("filterHash").GetString()!.ShouldMatch("^[0-9a-f]{4}$");
        ab.GetProperty("filterHash").GetString().ShouldBe(ba.GetProperty("filterHash").GetString());
        ab.GetProperty("filterHash").GetString().ShouldNotBe(none.GetProperty("filterHash").GetString());
    }

    [Theory]
    [InlineData("filter[ghost]=1", "filter[ghost]", "notFilterable")]
    [InlineData("filter[status][gt]=enrolled", "filter[status][gt]", "operatorNotAllowed")]
    [InlineData("filter[status][like]=enrolled", "filter[status][like]", "operatorNotAllowed")]
    [InlineData("filter[status]=enrolled&filter[status]=withdrawn", "filter[status]", "repeated")]
    [InlineData("filter[status]=graduated", "filter[status]", "invalidValue")]
    [InlineData("filter[balance][gt]=1,5", "filter[balance][gt]", "invalidValue")]
    [InlineData("filter[enrolledAt][lt]=2026-09-01T06:00:00", "filter[enrolledAt][lt]", "invalidValue")]
    [InlineData("filter[dateOfBirth][between]=2014-01-10", "filter[dateOfBirth][between]", "invalidRange")]
    [InlineData("sort=sectionId", "sort", "notSortable")]
    [InlineData("sort=id,lastName", "sort", "idMustBeLast")]
    [InlineData("sort=lastName,firstName,status,balance", "sort", "tooManyFields")]
    [InlineData("q=family", "q", "notSearchable")]
    public async Task An_undeclared_or_malformed_parameter_is_refused_naming_it(string query, string field, string code)
    {
        var (status, body) = await RosterClient.GetAsync(roster, Pupils + "?" + query);

        status.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        var error = body.GetProperty("errors")[0];
        error.GetProperty("field").GetString().ShouldBe(field);
        error.GetProperty("code").GetString().ShouldBe(code);
    }
}
