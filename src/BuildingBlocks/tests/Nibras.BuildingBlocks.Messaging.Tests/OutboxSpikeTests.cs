using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Messaging.Tests;

/// <summary>
/// The exit check of SL-MSG-001 and document 11 open point 5: one dispatcher leader driven at the Appendix N N-09 fan-out
/// rate at its scale set, 42,000 recipients across 10 campuses within the same minute, against the foundation outbox,
/// with the age of the oldest undispatched row read against the 5-second line. Explicit: it waits on wall time and is
/// timing-sensitive, so it is run by hand and its result recorded, not run in the pipeline.
/// </summary>
[Collection(MessagingDefinition.Name)]
public sealed class OutboxSpikeTests(MessagingFixture messaging)
{
    private const int Tenants = 10;
    private const int Messages = 42_000;
    private const int PerTick = 70; // one tenant's commit every 100 ms: 700 messages a second, 60 seconds

    [Fact(Explicit = true)]
    public async Task One_leader_keeps_the_oldest_pending_row_under_five_seconds_at_the_N09_fan_out_rate()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var queue = await QueueProbe.BindAsync(messaging.Connection, "probe.spike-test", "probe.item.#");
        using var host = await messaging.StartHostAsync();
        var tenants = Enumerable.Range(0, Tenants).Select(_ => Guid.CreateVersion7()).ToArray();
        var ages = new List<double>();
        var clock = TimeProvider.System;
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var sampler = Task.Run(async () =>
        {
            using var every = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
            await using var connection = new NpgsqlConnection(messaging.Database.SuperuserConnectionString);
            await connection.OpenAsync(ct);
            await using var oldest = new NpgsqlCommand("SELECT min(created_at) FROM probe.outbox_messages WHERE dispatched_at IS NULL AND tenant_id = ANY(@tenants)", connection);
            oldest.Parameters.AddWithValue("tenants", tenants);
            while (await every.WaitForNextTickAsync(stop.Token))
            {
                var value = await oldest.ExecuteScalarAsync(ct);
                ages.Add(value is DateTime at ? (clock.GetUtcNow() - new DateTimeOffset(at, TimeSpan.Zero)).TotalSeconds : 0);
            }
        }, ct);

        var started = clock.GetTimestamp();
        using (var tick = new PeriodicTimer(TimeSpan.FromMilliseconds(100)))
        {
            for (var written = 0; written < Messages; written += PerTick)
            {
                await tick.WaitForNextTickAsync(ct);
                var tenant = tenants[written / PerTick % Tenants];
                await MessagingFixture.CommitAsync(host, tenant, (publisher, _) =>
                {
                    for (var i = 0; i < PerTick; i++)
                    {
                        publisher.Enqueue(new ItemCreated(Guid.CreateVersion7(), "recipient", Guid.CreateVersion7()));
                    }
                });
            }
        }

        var writing = clock.GetElapsedTime(started);
        var received = 0;
        while (received < Messages)
        {
            await queue.NextAsync(seconds: 60);
            received++;
        }

        var drained = clock.GetElapsedTime(started);
        await stop.CancelAsync();
        try
        {
            await sampler;
        }
        catch (OperationCanceledException)
        {
            // the sampler stops when the run is over
        }

        var sorted = ages.Order().ToList();
        var p95 = sorted[(int)Math.Ceiling(sorted.Count * 0.95) - 1];
        TestContext.Current.TestOutputHelper?.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"{Messages} messages from {Tenants} tenants written in {writing.TotalSeconds:F1} s, all received {drained.TotalSeconds:F1} s after the first write; oldest pending row: max {sorted[^1]:F2} s, p95 {p95:F2} s over {sorted.Count} samples"));
        sorted[^1].ShouldBeLessThan(5, "document 11 open point 5: one leader holds the N-09 fan-out under the 5-second line");
    }
}
