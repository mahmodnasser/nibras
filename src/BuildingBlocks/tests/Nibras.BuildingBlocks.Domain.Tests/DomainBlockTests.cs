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

public sealed class EntityTests
{
    private sealed record Enrolled(DateTimeOffset OccurredAt) : IDomainEvent;

    private sealed class Student(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Enrol(DateTimeOffset at) => RaiseDomainEvent(new Enrolled(at));

        public void RaiseNothing() => RaiseDomainEvent(null!);
    }

    private sealed class Guardian(Guid id) : AggregateRoot<Guid>(id);

    private static readonly Guid Id = Guid.Parse("018f6a1e-5c2b-7d3e-9a4f-1b2c3d4e5f60");

    [Fact]
    public void Entities_of_one_type_with_one_id_are_equal_and_hash_alike()
    {
        object other = new Student(Id);

        new Student(Id).Equals(other).ShouldBeTrue();
        new Student(Id).GetHashCode().ShouldBe(other.GetHashCode());
        new Student(Id).Equals(new Student(Guid.CreateVersion7())).ShouldBeFalse();
        new Student(Id).Equals((object?)null).ShouldBeFalse();
    }

    [Fact]
    public void Entities_of_different_types_with_one_id_are_not_equal()
    {
        new Student(Id).Equals(new Guardian(Id)).ShouldBeFalse();
        new Student(Id).GetHashCode().ShouldNotBe(new Guardian(Id).GetHashCode());
    }

    [Fact]
    public void An_aggregate_keeps_its_raised_events_until_they_are_cleared()
    {
        var student = new Student(Id);
        var at = new DateTimeOffset(2026, 9, 27, 8, 0, 0, TimeSpan.Zero);

        student.Enrol(at);

        student.DomainEvents.ShouldHaveSingleItem().OccurredAt.ShouldBe(at);
        student.ClearDomainEvents();
        student.DomainEvents.ShouldBeEmpty();
        Should.Throw<ArgumentNullException>(student.RaiseNothing);
    }
}

public sealed class SystemClockTests
{
    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public void The_system_clock_reads_the_injected_time_provider()
    {
        var now = new DateTimeOffset(2026, 9, 27, 8, 0, 0, TimeSpan.Zero);

        new SystemClock(new FixedTime(now)).UtcNow.ShouldBe(now);
    }
}
