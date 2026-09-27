using Nibras.BuildingBlocks.Domain;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Domain.Tests;

public sealed class ErrorTests
{
    [Theory]
    [InlineData("PLATFORM_VALIDATION_FAILED")]
    [InlineData("ATTENDANCE_SESSION_LOCKED")]
    [InlineData("A1")]
    public void An_upper_snake_case_code_is_accepted(string code) =>
        new Error(code, "Readable message.").Code.ShouldBe(code);

    [Theory]
    [InlineData("platform_validation_failed")]
    [InlineData("_LEADING")]
    [InlineData("TRAILING_")]
    [InlineData("DOUBLE__UNDERSCORE")]
    [InlineData("HAS SPACE")]
    [InlineData("HYPHEN-CODE")]
    public void A_code_that_is_not_upper_snake_case_is_refused(string code) =>
        Should.Throw<ArgumentException>(() => new Error(code, "Readable message."));
}

public sealed class ResultTests
{
    private static readonly Error Locked = new("ATTENDANCE_SESSION_LOCKED", "The session is locked.");

    [Fact]
    public void A_success_carries_its_value_and_no_error()
    {
        var result = Result.Success(28);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(28);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Reading_the_value_of_a_failure_throws_and_names_the_code()
    {
        var result = Result.Failure<int>(Locked);

        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => result.Value).Message.ShouldContain("ATTENDANCE_SESSION_LOCKED");
    }
}

public sealed class ValueObjectTests
{
    private sealed class Range(int from, int to) : ValueObject
    {
        protected override IEnumerable<object?> EqualityComponents()
        {
            yield return from;
            yield return to;
        }
    }

    [Fact]
    public void Value_objects_with_equal_components_are_equal()
    {
        new Range(1, 5).ShouldBe(new Range(1, 5));
        new Range(1, 5).GetHashCode().ShouldBe(new Range(1, 5).GetHashCode());
        new Range(1, 5).ShouldNotBe(new Range(1, 6));
    }
}

public sealed class IdGeneratorTests
{
    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public void A_later_id_sorts_after_an_earlier_one_and_is_version_7()
    {
        var earlier = new UuidV7Generator(new FixedTime(new DateTimeOffset(2026, 9, 27, 8, 0, 0, TimeSpan.Zero))).NewId();
        var later = new UuidV7Generator(new FixedTime(new DateTimeOffset(2026, 9, 27, 8, 0, 1, TimeSpan.Zero))).NewId();

        earlier.Version.ShouldBe(7);
        string.CompareOrdinal(earlier.ToString("N"), later.ToString("N")).ShouldBeLessThan(0);
    }
}
