using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nibras.BuildingBlocks.Tenancy;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Messaging.Tests;

/// <summary>The dispatcher of document 11 part 8.3 against the real broker and PostgreSQL behind PgBouncer.</summary>
[Collection(MessagingDefinition.Name)]
public sealed class DispatcherTests(MessagingFixture messaging)
{
    [Fact]
    public async Task Each_tenant_is_read_under_its_own_tenant_and_published_with_it()
    {
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.tenants-test", "probe.item.#");
        using var host = await messaging.StartHostAsync();
        var (tenantA, tenantB) = (Guid.CreateVersion7(), Guid.CreateVersion7());

        await MessagingFixture.CommitAsync(host, tenantA, (publisher, _) => publisher.Enqueue(new ItemCreated(Guid.CreateVersion7(), "a", Guid.CreateVersion7())));
        await MessagingFixture.CommitAsync(host, tenantB, (publisher, _) => publisher.Enqueue(new ItemCreated(Guid.CreateVersion7(), "b", Guid.CreateVersion7())));
        var first = await queue.NextAsync();
        var second = await queue.NextAsync();

        new[] { first, second }.ToDictionary(d => d.Header("x-nibras-tenant-id")!, d => d.Text).ShouldSatisfyAllConditions(
            byTenant => byTenant[tenantA.ToString("D")].ShouldContain("\"name\":\"a\""),
            byTenant => byTenant[tenantB.ToString("D")].ShouldContain("\"name\":\"b\""));
    }

    [Fact]
    [Trait("TestCase", "TC-MSG-971")]
    public async Task An_event_no_queue_binds_lands_in_the_unrouted_queue_rather_than_vanishing()
    {
        await using var unrouted = await QueueProbe.BindAsync(messaging.Connection, MessagingTopology.UnroutedQueue("Probe"), bindingKey: "", arguments: UnroutedArguments());
        using var host = await messaging.StartHostAsync();
        var stray = new StrayHappened(Guid.CreateVersion7());

        await MessagingFixture.CommitAsync(host, Guid.CreateVersion7(), (publisher, _) => publisher.Enqueue(stray));
        var caught = await unrouted.NextAsync();

        caught.RoutingKey.ShouldBe("probe.stray.happened.v1");
        caught.Text.ShouldContain(stray.Id.ToString("D"));
        await MessagingFixture.UntilAsync(host.Services.GetRequiredService<OutboxDispatcher>(), async () => (await messaging.RowAsync(stray.Id.ToString("D")))!.Value.DispatchedAt is not null);
    }

    [Fact]
    [Trait("TestCase", "TC-MSG-959")]
    public async Task A_full_queue_refuses_the_publish_the_outbox_keeps_the_row_and_later_rows_of_its_key_wait_behind_it()
    {
        var full = new Dictionary<string, object?>(StringComparer.Ordinal) { ["x-queue-type"] = "quorum", ["x-max-length"] = 1, ["x-overflow"] = "reject-publish" };
        await using var limited = await QueueProbe.BindAsync(messaging.Connection, "probe.full-test", "probe.full.#", full, consume: false);
        await using var side = await QueueProbe.BindAsync(messaging.Connection, "probe.side-test", "probe.side.#");
        var failures = new List<string>();
        using var listener = Listen("nibras_outbox_publish_failures_total", (_, tags) =>
        {
            lock (failures)
            {
                failures.Add(Tag(tags, "reason"));
            }
        });
        using var host = await messaging.StartHostAsync();
        var dispatcher = host.Services.GetRequiredService<OutboxDispatcher>();
        var (refused, behind, other) = (new QueueFilled(Guid.CreateVersion7(), "k1"), new SideNoted(Guid.CreateVersion7(), "k1"), new SideNoted(Guid.CreateVersion7(), "k2"));
        await limited.FillAsync(); // the queue is refusing publishes when the dispatcher publishes
        var filled = await limited.CountAsync();

        await MessagingFixture.CommitAsync(host, Guid.CreateVersion7(), (publisher, _) =>
        {
            publisher.Enqueue(refused);
            publisher.Enqueue(behind);
            publisher.Enqueue(other);
        });
        await MessagingFixture.UntilAsync(dispatcher, async () => (await messaging.RowAsync(refused.Id.ToString("D")))!.Value.Attempts > 0);

        var row = (await messaging.RowAsync(refused.Id.ToString("D")))!.Value;
        row.DispatchedAt.ShouldBeNull();
        row.LastError.ShouldNotBeNull().ShouldStartWith("negative confirm");
        var held = (await messaging.RowAsync(behind.Id.ToString("D")))!.Value;
        held.DispatchedAt.ShouldBeNull("a confirmed row behind a refused row of its key is published again after it");
        held.Attempts.ShouldBe(0);
        (await messaging.RowAsync(other.Id.ToString("D")))!.Value.DispatchedAt.ShouldNotBeNull("another key is not held back");
        (await limited.CountAsync()).ShouldBe(filled, "the refused message never entered the queue");
        lock (failures)
        {
            failures.ShouldContain("nack");
        }

        await limited.PurgeAsync();
        await MessagingFixture.UntilAsync(dispatcher, async () => (await messaging.RowAsync(behind.Id.ToString("D")))!.Value.DispatchedAt is not null);

        (await messaging.RowAsync(refused.Id.ToString("D")))!.Value.DispatchedAt.ShouldNotBeNull();
        var onSide = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            onSide.Add((await side.NextAsync()).Text);
        }

        onSide.Count(text => text.Contains(behind.Id.ToString("D"), StringComparison.Ordinal)).ShouldBe(2, "published before the refusal and again after it; the inbox absorbs the copy");
    }

    [Fact]
    [Trait("TestCase", "TC-MSG-753")]
    public async Task A_row_is_never_published_before_an_earlier_row_whose_transaction_commits_later()
    {
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.horizon-test", "probe.item.#");
        using var host = await messaging.StartHostAsync();
        var dispatcher = host.Services.GetRequiredService<OutboxDispatcher>();
        var tenant = Guid.CreateVersion7();
        var earlier = new ItemCreated(Guid.CreateVersion7(), "earlier", Guid.CreateVersion7());
        var later = new ItemCreated(Guid.CreateVersion7(), "later", Guid.CreateVersion7());
        var release = new TaskCompletionSource();
        var saved = new TaskCompletionSource();

        var slow = Task.Run(async () =>
        {
            await using var scope = host.Services.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<TenantContext>().Set(new TenantId(tenant));
            var db = scope.ServiceProvider.GetRequiredService<ProbeDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
            scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>().Enqueue(earlier);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            saved.SetResult();
            await release.Task;
            await transaction.CommitAsync(TestContext.Current.CancellationToken);
        }, TestContext.Current.CancellationToken);
        try
        {
            await saved.Task.WaitAsync(TimeSpan.FromSeconds(20), TestContext.Current.CancellationToken);
            await MessagingFixture.CommitAsync(host, tenant, (publisher, _) => publisher.Enqueue(later));
            await MessagingFixture.RoundsAsync(dispatcher, 2);
            queue.TryNext(out _).ShouldBeFalse("the later row waits while an earlier transaction is still running");
        }
        finally
        {
            release.TrySetResult(); // never leave the transaction open for the next test
            await slow;
        }

        (await queue.NextAsync()).Text.ShouldContain("earlier");
        (await queue.NextAsync()).Text.ShouldContain("later");
    }

    [Fact]
    [Trait("TestCase", "TC-MSG-751")]
    public async Task One_host_leads_and_a_standby_takes_the_lock_when_the_leader_stops()
    {
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.leader-test", "probe.item.#");
        var first = await messaging.StartHostAsync(waitForLeader: false);
        var second = await messaging.StartHostAsync(waitForLeader: false);
        var (one, two) = (first.Services.GetRequiredService<OutboxDispatcher>(), second.Services.GetRequiredService<OutboxDispatcher>());
        try
        {
            await MessagingFixture.UntilAsync(one, () => Task.FromResult(one.IsLeader || two.IsLeader));
            await MessagingFixture.RoundsAsync(two, 1);
            (one.IsLeader ^ two.IsLeader).ShouldBeTrue("exactly one host holds the lock");
            var (leader, standby, standbyHost) = one.IsLeader ? (first, two, second) : (second, one, first);

            await leader.StopAsync(TestContext.Current.CancellationToken);
            leader.Dispose();
            await MessagingFixture.UntilAsync(standby, () => Task.FromResult(standby.IsLeader));
            var created = new ItemCreated(Guid.CreateVersion7(), "after the failover", Guid.CreateVersion7());
            await MessagingFixture.CommitAsync(standbyHost, Guid.CreateVersion7(), (publisher, _) => publisher.Enqueue(created));

            (await queue.NextAsync()).Text.ShouldContain("after the failover");
            await MessagingFixture.RoundsAsync(standby, 2);
            queue.TryNext(out _).ShouldBeFalse();
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    [Fact]
    public async Task With_the_broker_unreachable_the_dispatcher_keeps_retrying_and_the_row_stays_pending()
    {
        using var host = await messaging.StartHostAsync(broker: new Uri("amqp://nibras:nibras-test-only@127.0.0.1:1/"), waitForLeader: false);
        var dispatcher = host.Services.GetRequiredService<OutboxDispatcher>();
        var stray = new StrayHappened(Guid.CreateVersion7());

        await MessagingFixture.CommitAsync(host, Guid.CreateVersion7(), (publisher, _) => publisher.Enqueue(stray));
        await MessagingFixture.RoundsAsync(dispatcher, 2);

        dispatcher.IsLeader.ShouldBeFalse();
        (await messaging.RowAsync(stray.Id.ToString("D")))!.Value.DispatchedAt.ShouldBeNull();
    }

    [Fact]
    public async Task The_backlog_gauges_and_the_dispatched_counter_carry_the_service_label()
    {
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.metrics-test", "probe.item.#");
        var dispatched = new List<(long Value, string Service, string Exchange)>();
        using var counter = Listen("nibras_outbox_dispatched_total", (value, tags) =>
        {
            lock (dispatched)
            {
                dispatched.Add((value, Tag(tags, "service"), Tag(tags, "exchange")));
            }
        });
        var gauges = new Dictionary<string, double>(StringComparer.Ordinal);
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Name is "nibras_outbox_pending_messages" or "nibras_outbox_oldest_pending_seconds")
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<double>((instrument, value, _, _) =>
        {
            lock (gauges)
            {
                gauges[instrument.Name] = value;
            }
        });
        listener.Start();
        using var host = await messaging.StartHostAsync();

        await MessagingFixture.CommitAsync(host, Guid.CreateVersion7(), (publisher, _) => publisher.Enqueue(new ItemCreated(Guid.CreateVersion7(), "counted", Guid.CreateVersion7())));
        await queue.NextAsync();
        await MessagingFixture.RoundsAsync(host.Services.GetRequiredService<OutboxDispatcher>(), 1);
        listener.RecordObservableInstruments();

        lock (dispatched)
        {
            dispatched.ShouldContain((1L, "Probe", "nibras.probe"));
        }

        lock (gauges)
        {
            gauges.Keys.ShouldBe(["nibras_outbox_pending_messages", "nibras_outbox_oldest_pending_seconds"], ignoreOrder: true);
            gauges["nibras_outbox_oldest_pending_seconds"].ShouldBeGreaterThanOrEqualTo(0);
        }
    }

    private static Dictionary<string, object?> UnroutedArguments() => new(StringComparer.Ordinal)
    {
        ["x-queue-type"] = "quorum",
        ["x-max-length"] = MessagingTopology.UnroutedQueueLength,
        ["x-overflow"] = "drop-head",
    };

    private delegate void Measured(long value, ReadOnlySpan<KeyValuePair<string, object?>> tags);

    private static string Tag(ReadOnlySpan<KeyValuePair<string, object?>> tags, string key)
    {
        foreach (var tag in tags)
        {
            if (tag.Key == key)
            {
                return (string)tag.Value!;
            }
        }

        throw new KeyNotFoundException(key);
    }

    private static MeterListener Listen(string instrumentName, Measured measured)
    {
        var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Name == instrumentName)
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) => measured(value, tags));
        listener.Start();
        return listener;
    }
}
