using Microsoft.EntityFrameworkCore;
using Nibras.BuildingBlocks.Domain;
using Nibras.BuildingBlocks.Persistence;
using Nibras.Contracts.Shared;

namespace Nibras.BuildingBlocks.Messaging.Tests;

/// <summary>A tenant-owned row of the throwaway service "Probe", so a handler has a state change to commit beside its event.</summary>
public sealed class ProbeItem : AggregateRoot<Guid>, ITenantEntity
{
    public ProbeItem(Guid id, string name)
        : base(id) => Name = name;

    public Guid TenantId { get; private set; }

    public string Name { get; private set; }
}

public sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options) : NibrasDbContext(options)
{
    public DbSet<ProbeItem> Items => Set<ProbeItem>();

    protected override string Schema => "probe";

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProbeItem>(item =>
        {
            item.ToTable("items");
            item.HasKey(i => i.Id);
            item.Property(i => i.Name).HasMaxLength(100);
            item.Ignore(i => i.DomainEvents);
        });
        modelBuilder.AddNibrasOutbox();
    }
}

[IntegrationEvent("probe.item.created.v1")]
public sealed record ItemCreated(Guid ItemId, string Name, [property: PartitionKey] Guid SubjectId);

[IntegrationEvent("probe.stray.happened.v1")]
public sealed record StrayHappened(Guid Id);

[IntegrationEvent("probe.full.filled.v1")]
public sealed record QueueFilled(Guid Id, [property: PartitionKey] string Key);

[IntegrationEvent("probe.side.noted.v12")]
public sealed record SideNoted(Guid Id, [property: PartitionKey] string Key);

[IntegrationEvent("attendance.student.absent.v1")]
public sealed record AnotherServicesEvent(Guid Id);

[IntegrationEvent("probe.item_created")]
public sealed record MalformedEvent(Guid Id);

[IntegrationEvent("probe.item.doubled.v1")]
public sealed record TwoKeys([property: PartitionKey] string First, [property: PartitionKey] string Second);

public sealed record NotAnEvent(Guid Id);

public sealed class FixedUser(Guid userId) : ICurrentUser
{
    public Guid? UserId { get; } = userId;
}
