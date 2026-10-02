using Nibras.BuildingBlocks.Application.Lists;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Application.Tests;

public enum Stage
{
    Draft,
    Published,
}

public sealed record Row(Guid Id, string Name, string? Note, Stage Stage, bool IsActive, DateOnly On, int Count);

/// <summary>A field map refuses at declaration what the grammar could never honour (document 22 §3).</summary>
[Trait("TestCase", "TC-API-010")]
public sealed class ListFieldMapTests
{
    private static ListFieldMap<Row> Map(int max = 200) => ListFieldMap.For<Row>(r => r.Id, "+name", max);

    [Theory]
    [InlineData(500, 200)]
    [InlineData(201, 200)]
    [InlineData(200, 200)]
    [InlineData(20, 20)]
    public void The_declared_maximum_page_size_never_exceeds_200(int declared, int effective) =>
        Map(declared).MaxPageSize.ShouldBe(effective);

    [Fact]
    public void The_id_is_always_declared_sortable_and_filterable_by_equality()
    {
        var id = Map().Fields["id"];

        id.Sortable.ShouldBeTrue();
        id.Operators.ShouldBe(FilterOperators.Eq | FilterOperators.In);
    }

    [Fact]
    public void A_valid_declaration_records_the_type_and_nullability()
    {
        var map = Map()
            .Field("name", r => r.Name, FilterOperators.Text, sortable: true)
            .Field("note", r => r.Note, FilterOperators.Text | FilterOperators.IsNull)
            .Field("count", r => (int?)r.Count, FilterOperators.Range | FilterOperators.IsNull);

        map.Fields["count"].CoreType.ShouldBe(typeof(int));
        map.Fields["count"].IsNullable.ShouldBeTrue();
        map.Fields["name"].Sortable.ShouldBeTrue();
    }

    [Fact]
    public void Ordering_operators_on_an_enum_a_boolean_or_an_id_are_refused() =>
        Should.Throw<ArgumentException>(() => Map().Field("stage", r => r.Stage, FilterOperators.Range));

    [Fact]
    public void Text_operators_on_a_non_text_field_are_refused() =>
        Should.Throw<ArgumentException>(() => Map().Field("on", r => r.On, FilterOperators.Text));

    [Fact]
    public void IsNull_on_a_field_that_can_never_be_null_is_refused() =>
        Should.Throw<ArgumentException>(() => Map().Field("count", r => r.Count, FilterOperators.IsNull));

    [Theory]
    [InlineData("Name")]
    [InlineData("first_name")]
    [InlineData("a.b.c")]
    [InlineData("")]
    public void A_name_that_is_not_camelCase_of_at_most_two_levels_is_refused(string name) =>
        Should.Throw<ArgumentException>(() => Map().Field(name, r => r.Name));

    [Fact]
    public void A_field_declared_twice_is_refused() =>
        Should.Throw<ArgumentException>(() => Map().Field("name", r => r.Name).Field("name", r => r.Name));
}
