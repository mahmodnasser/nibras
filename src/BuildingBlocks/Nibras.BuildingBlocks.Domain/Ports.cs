namespace Nibras.BuildingBlocks.Domain;

/// <summary>The clock. Domain code never reads <see cref="DateTimeOffset.UtcNow"/>, so tests control time.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>The system clock over <see cref="TimeProvider"/>.</summary>
public sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
}

/// <summary>Generates identifiers. Every key in the platform is a UUID v7, time-ordered for index locality.</summary>
public interface IIdGenerator
{
    Guid NewId();
}

/// <summary>UUID v7 from the clock, so ids created later sort later.</summary>
public sealed class UuidV7Generator(TimeProvider timeProvider) : IIdGenerator
{
    public Guid NewId() => Guid.CreateVersion7(timeProvider.GetUtcNow());
}
