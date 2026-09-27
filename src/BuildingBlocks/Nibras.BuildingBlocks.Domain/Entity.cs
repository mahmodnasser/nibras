namespace Nibras.BuildingBlocks.Domain;

/// <summary>An object with identity. Two entities are equal when their ids are equal.</summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    public TId Id { get; }

    public bool Equals(Entity<TId>? other) =>
        other is not null && other.GetType() == GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

/// <summary>Something that happened inside one aggregate. Never published across services: contracts do that.</summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

/// <summary>The consistency boundary. Only an aggregate root raises domain events.</summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}

/// <summary>A value defined only by its components, compared component by component.</summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> EqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && other.GetType() == GetType() && EqualityComponents().SequenceEqual(other.EqualityComponents());

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in EqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }
}
