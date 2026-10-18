using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nibras.BuildingBlocks.Persistence;
using Nibras.BuildingBlocks.Tenancy;
using RabbitMQ.Client;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Messaging.Tests;

/// <summary>The outbox write and the envelope on the wire (document 11 parts 2.1 and 8.1 to 8.2).</summary>
[Collection(MessagingDefinition.Name)]
public sealed class OutboxTests(MessagingFixture messaging)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    [Trait("TestCase", "TC-MSG-953")]
    public async Task A_rolled_back_handler_publishes_nothing_and_a_committed_one_publishes_once()
    {
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.outbox-test", "probe.item.#");
        using var host = await messaging.StartHostAsync();
        var tenant = Guid.CreateVersion7();
        var rolledBack = new ItemCreated(Guid.CreateVersion7(), "rolled back", Guid.CreateVersion7());
        var committed = new ItemCreated(Guid.CreateVersion7(), "committed", Guid.CreateVersion7());

        await MessagingFixture.CommitAsync(host, tenant, (publisher, _) => publisher.Enqueue(rolledBack), commit: false);
        await MessagingFixture.CommitAsync(host, tenant, (publisher, _) => publisher.Enqueue(committed));

        var delivered = await queue.NextAsync();
        delivered.Text.ShouldContain(committed.ItemId.ToString("D"));
        (await messaging.RowAsync(rolledBack.ItemId.ToString("D"))).ShouldBeNull();
        await MessagingFixture.RoundsAsync(host.Services.GetRequiredService<OutboxDispatcher>(), 2);
        queue.TryNext(out _).ShouldBeFalse();
        (await messaging.RowAsync(committed.ItemId.ToString("D")))!.Value.DispatchedAt.ShouldNotBeNull();
    }

    /// <summary>REQ-MSG-013 on the wire; the schema baseline of every record, TC-TST-205, comes with SL-MSG-004.</summary>
    [Fact]
    public async Task The_envelope_travels_in_the_properties_and_headers_document_11_names()
    {
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.envelope-test", "probe.item.created.v1");
        var user = Guid.CreateVersion7();
        using var host = await messaging.StartHostAsync(user: new FixedUser(user));
        var tenant = Guid.CreateVersion7();
        var correlation = Guid.CreateVersion7();
        var causation = Guid.CreateVersion7();
        var created = new ItemCreated(Guid.CreateVersion7(), "Layla", Guid.CreateVersion7());

        await MessagingFixture.CommitAsync(host, tenant, (publisher, context) =>
        {
            context.CorrelationId = correlation;
            context.CausationId = causation;
            publisher.Enqueue(created);
        });
        var delivered = await queue.NextAsync();

        delivered.RoutingKey.ShouldBe("probe.item.created.v1");
        Guid.Parse(delivered.Properties.MessageId!).Version.ShouldBe(7);
        delivered.Properties.CorrelationId.ShouldBe(correlation.ToString("D"));
        delivered.Properties.Type.ShouldBe(typeof(ItemCreated).FullName);
        delivered.Properties.ContentType.ShouldBe("application/json");
        delivered.Properties.DeliveryMode.ShouldBe(DeliveryModes.Persistent);
        delivered.Header("x-nibras-tenant-id").ShouldBe(tenant.ToString("D"));
        delivered.Header("x-nibras-causation-id").ShouldBe(causation.ToString("D"));
        delivered.Header("x-nibras-user-id").ShouldBe(user.ToString("D"));
        delivered.Header("x-nibras-schema-version").ShouldBe("1");
        delivered.Header("x-partition-key").ShouldBe(created.SubjectId.ToString("D"));
        var occurredAt = DateTimeOffset.Parse(delivered.Header("x-nibras-occurred-at")!, System.Globalization.CultureInfo.InvariantCulture);
        delivered.Header("x-nibras-occurred-at")!.ShouldMatch(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{6}Z$");
        delivered.Properties.Timestamp.UnixTime.ShouldBe(occurredAt.ToUnixTimeSeconds());
        var body = JsonNode.Parse(delivered.Text)!.AsObject();
        body.Select(p => p.Key).ShouldBe(["itemId", "name", "subjectId"], ignoreOrder: true);
        body["name"]!.GetValue<string>().ShouldBe("Layla");
    }

    [Fact]
    [Trait("TestCase", "TC-MSG-955")]
    public async Task Enqueue_refuses_a_type_that_is_not_one_well_named_event_of_this_service()
    {
        using var host = await messaging.StartHostAsync();
        await using var scope = host.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TenantContext>().Set(new TenantId(Guid.CreateVersion7()));
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        Should.Throw<ArgumentException>(() => publisher.Enqueue(new NotAnEvent(Guid.CreateVersion7()))).Message.ShouldContain("is not an integration event");
        Should.Throw<ArgumentException>(() => publisher.Enqueue(new AnotherServicesEvent(Guid.CreateVersion7()))).Message.ShouldContain("belongs to another service");
        Should.Throw<ArgumentException>(() => publisher.Enqueue(new MalformedEvent(Guid.CreateVersion7()))).Message.ShouldContain("'probe.item_created' is not <service>.<entity>.<event>.v<n>");
        Should.Throw<ArgumentException>(() => publisher.Enqueue(new TwoKeys("a", "b"))).Message.ShouldContain("2 properties with [PartitionKey]");
        scope.ServiceProvider.GetRequiredService<ProbeDbContext>().ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact]
    public async Task The_outbox_keeps_the_tenant_barriers_and_carries_no_audit_or_soft_delete_columns()
    {
        using var host = await messaging.StartHostAsync();
        await using var scope = host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
        var outbox = db.Model.FindEntityType(typeof(OutboxMessage))!;

        outbox.GetTableName().ShouldBe("outbox_messages");
        db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(OutboxMessage))!.IsTableExcludedFromMigrations().ShouldBeTrue();
        outbox.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()).ShouldBe(["tenant_id", "created_at", "id"]);
        outbox.GetDeclaredQueryFilters().Select(f => f.Key).ShouldBe([QueryFilters.Tenant]);
        foreach (var audit in new[] { AuditColumns.CreatedBy, AuditColumns.UpdatedAt, AuditColumns.UpdatedBy, AuditColumns.DeletedAt, AuditColumns.DeletedBy, AuditColumns.Xmin })
        {
            outbox.FindProperty(audit).ShouldBeNull(audit);
        }
    }

    [Fact]
    public async Task A_service_whose_model_maps_no_outbox_is_told_how_to_map_it()
    {
        var services = new ServiceCollection();
        services.AddNibrasDbContext<NoOutboxDbContext>("Host=127.0.0.1;Port=1;Database=x;Username=x;Password=x");
        services.AddNibrasMessaging<NoOutboxDbContext>(o =>
        {
            o.Service = "Probe";
            o.Broker = messaging.Broker;
            o.ApplicationConnectionString = "Host=x";
            o.SessionConnectionString = "Host=x";
        });
        services.AddLogging();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        Should.Throw<InvalidOperationException>(() => publisher.Enqueue(new StrayHappened(Guid.CreateVersion7())))
            .Message.ShouldContain("call modelBuilder.AddNibrasOutbox()");
    }
}

public sealed class NoOutboxDbContext(DbContextOptions<NoOutboxDbContext> options) : NibrasDbContext(options)
{
    protected override string Schema => "nooutbox";

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
    }
}
