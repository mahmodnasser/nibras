using Nibras.BuildingBlocks.Application.Lists;
using Nibras.BuildingBlocks.Web.Lists;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>The limits and parsing edges of the grammar (document 22 §3), without a database.</summary>
[Trait("TestCase", "TC-API-010")]
public sealed class FilterSortGrammarTests
{
    private static (ListQuery? Query, IReadOnlyList<FieldError> Errors) Parse(string query) => FilterSortGrammar.Parse(query, Pupils.Fields);

    [Fact]
    public void A_leading_plus_that_arrived_as_a_space_is_still_ascending()
    {
        var (query, _) = Parse("?sort=%2BlastName,-balance");
        var (spaced, _) = Parse("?sort=+lastName,-balance");

        query!.NormalizedSort.ShouldBe("+lastName,-balance,+id");
        spaced!.NormalizedSort.ShouldBe("+lastName,-balance,+id");
    }

    [Fact]
    public void An_explicit_trailing_id_is_kept_with_its_direction()
    {
        Parse("sort=lastName,-id").Query!.NormalizedSort.ShouldBe("+lastName,-id");
    }

    [Fact]
    public void Values_are_typed_by_the_declared_field_with_the_invariant_culture()
    {
        var (query, errors) = Parse("filter[balance][between]=1.50..20&filter[status][in]=enrolled,withdrawn&filter[enrolledAt][gte]=2026-09-01T05:00:00Z");

        errors.ShouldBeEmpty();
        query!.Filters.Single(f => f.Field == "balance").Values.ShouldBe([1.50m, 20m]);
        query.Filters.Single(f => f.Field == "status").Values.ShouldBe([PupilStatus.Enrolled, PupilStatus.Withdrawn]);
        query.Filters.Single(f => f.Field == "enrolledAt").Values.ShouldBe([new DateTimeOffset(2026, 9, 1, 5, 0, 0, TimeSpan.Zero)]);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void At_most_ten_predicates(int count, bool accepted)
    {
        // Ten distinct field and operator pairs, so only the count can be the reason for a refusal.
        string[] all =
        [
            "filter[lastName]=x", "filter[lastName][ne]=x", "filter[firstName]=x", "filter[firstName][ne]=x",
            "filter[nickname]=x", "filter[nickname][ne]=x", "filter[balance][gt]=1", "filter[balance][lt]=9",
            "filter[dateOfBirth][gte]=2014-01-01", "filter[dateOfBirth][lte]=2014-12-31", "filter[status]=enrolled",
        ];
        var parts = all.Take(count);

        var (query, errors) = Parse(string.Join('&', parts));

        (query is not null).ShouldBe(accepted, string.Join(", ", errors.Select(e => e.Field + ":" + e.Code)));
        if (!accepted)
        {
            errors.Single().Code.ShouldBe("tooManyPredicates");
        }
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void At_most_a_hundred_members_in_a_list(int count, bool accepted)
    {
        var (query, _) = Parse("filter[lastName][in]=" + string.Join(',', Enumerable.Range(0, count).Select(i => "n" + i)));

        (query is not null).ShouldBe(accepted);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void At_most_two_hundred_characters_in_a_value(int length, bool accepted)
    {
        var (query, errors) = Parse("filter[lastName]=" + new string('a', length));

        (query is not null).ShouldBe(accepted);
        if (!accepted)
        {
            errors.Single().Code.ShouldBe("maxLength");
        }
    }

    [Fact]
    public void A_cursor_longer_than_512_characters_is_refused()
    {
        Parse("cursor=" + new string('a', 512)).Query.ShouldNotBeNull();
        Parse("cursor=" + new string('a', 513)).Errors.Single().Field.ShouldBe("cursor");
    }

    [Fact]
    public void A_broken_default_sort_is_a_defect_found_on_the_first_request()
    {
        var broken = ListFieldMap.For<RosterPupil>(p => p.Id, "+surname");

        Should.Throw<InvalidOperationException>(() => FilterSortGrammar.Parse("", broken));
    }
}
