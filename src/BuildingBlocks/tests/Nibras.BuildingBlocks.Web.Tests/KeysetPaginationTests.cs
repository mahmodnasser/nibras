using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Application.Lists;
using Nibras.BuildingBlocks.Persistence;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>Keyset lists end to end: HTTP, the grammar, the cursor, EF Core and PostgreSQL 18.6.</summary>
[Collection(RosterDefinition.Name)]
public sealed class KeysetPaginationTests(RosterFixture roster)
{
    private const string Pupils = "/api/v1/probe/pupils";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    [Trait("TestCase", "TC-API-001")]
    public async Task A_page_size_above_the_maximum_is_clamped_to_200_not_refused()
    {
        var (status, page) = await RosterClient.GetAsync(roster, Pupils + "?pageSize=1000");

        status.ShouldBe(HttpStatusCode.OK);
        RosterClient.ShouldBeEnvelope(page);
        page.GetProperty("pageSize").GetInt32().ShouldBe(200);
        page.GetProperty("items").GetArrayLength().ShouldBe(200);
        page.GetProperty("hasMore").GetBoolean().ShouldBeTrue();
    }

    [Theory]
    [Trait("TestCase", "TC-API-001")]
    [InlineData("", 50)]
    [InlineData("?pageSize=199", 199)]
    [InlineData("?pageSize=200", 200)]
    [InlineData("?pageSize=201", 200)]
    [InlineData("?pageSize=99999999999999", 200)]
    public async Task The_default_is_50_and_the_maximum_holds_at_the_value_one_below_and_one_above(string query, int expected)
    {
        var (status, page) = await RosterClient.GetAsync(roster, Pupils + query);

        status.ShouldBe(HttpStatusCode.OK);
        page.GetProperty("pageSize").GetInt32().ShouldBe(expected);
        page.GetProperty("items").GetArrayLength().ShouldBe(expected);
    }

    [Theory]
    [Trait("TestCase", "TC-API-001")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("ten")]
    public async Task A_page_size_that_is_not_a_positive_number_is_refused(string pageSize)
    {
        var (status, body) = await RosterClient.GetAsync(roster, Pupils + "?pageSize=" + pageSize);

        status.ShouldBe(HttpStatusCode.BadRequest);
        body.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        body.GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("pageSize");
    }

    public static TheoryData<string, string> Sorts => new()
    {
        { "", "+lastName,+id" },
        { "-enrolledAt", "-enrolledAt,+id" },
        { "nickname", "+nickname,+id" },
        { "-nickname", "-nickname,+id" },
        { "-balance,firstName", "-balance,+firstName,+id" },
        { "status,-dateOfBirth,lastName", "+status,-dateOfBirth,+lastName,+id" },
    };

    [Theory]
    [Trait("TestCase", "TC-API-002")]
    [MemberData(nameof(Sorts))]
    public async Task Walking_forwards_and_backwards_visits_every_row_exactly_once_in_the_database_order(string sort, string normalized)
    {
        var query = Pupils + "?pageSize=50" + (sort.Length == 0 ? "" : "&sort=" + Uri.EscapeDataString(sort));

        var forward = await RosterClient.AllPagesAsync(roster, query);

        forward.Count.ShouldBe(21);
        forward.ShouldAllBe(p => p.GetProperty("sort").GetString() == normalized);
        forward[0].GetProperty("previousCursor").ValueKind.ShouldBe(JsonValueKind.Null);
        forward.Take(20).ShouldAllBe(p => p.GetProperty("items").GetArrayLength() == 50);
        var forwardIds = forward.SelectMany(RosterClient.Ids).ToList();
        forwardIds.Count.ShouldBe(RosterFixture.TenantACount);
        forwardIds.Distinct().Count().ShouldBe(RosterFixture.TenantACount);
        forwardIds.ToHashSet().SetEquals(roster.Seeded.Select(p => p.Id)).ShouldBeTrue("no row of tenant B, none missing");
        forwardIds.ShouldBe(await DatabaseOrderAsync(sort.Length == 0 ? "lastName" : sort));

        // Back from the last page to the first, page by page, through previousCursor.
        var backward = new List<JsonElement> { forward[^1] };
        while (backward[0].GetProperty("previousCursor").GetString() is { } previous)
        {
            var (status, page) = await RosterClient.GetAsync(roster, RosterClient.WithCursor(query, previous));
            status.ShouldBe(HttpStatusCode.OK);
            RosterClient.ShouldBeEnvelope(page);
            page.GetProperty("hasMore").GetBoolean().ShouldBeTrue();
            backward.Insert(0, page);
            backward.Count.ShouldBeLessThan(30);
        }

        backward.Select(p => RosterClient.Ids(p).ToList()).ShouldBe(forward.Select(p => RosterClient.Ids(p).ToList()));
    }

    [Fact]
    [Trait("TestCase", "TC-PERF-964")]
    public async Task One_untracked_command_returns_200_projected_rows_and_a_cursor_whatever_the_page_size_requested()
    {
        var (status, page) = await RosterClient.GetAsync(roster, Pupils + "?pageSize=1000&sort=-enrolledAt");
        status.ShouldBe(HttpStatusCode.OK);
        page.GetProperty("items").GetArrayLength().ShouldBe(200);
        page.GetProperty("nextCursor").GetString().ShouldNotBeNullOrEmpty();

        await using var scope = roster.ScopeFor(RosterFixture.TenantA);
        var db = scope.ServiceProvider.GetRequiredService<RosterDbContext>();
        var request = new KeysetRequest(200, [new SortField("lastName", false), new SortField("id", false)], [new FilterPredicate("status", FilterOperators.Ne, [PupilStatus.Withdrawn])]);
        using var counter = new CommandCounter();

        var result = await db.Pupils.ToKeysetPageAsync(request, Web.Tests.Pupils.Fields, Web.Tests.Pupils.ToRow, Ct);

        counter.Pupils.ShouldBe(1);
        result.Items.Count.ShouldBe(200);
        result.HasMoreInDirection.ShouldBeTrue();
        result.LastKey!.Count.ShouldBe(2);
        db.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact]
    [Trait("TestCase", "TC-API-002")]
    public async Task A_cursor_is_bound_to_its_endpoint_sort_and_filter_and_refused_when_edited()
    {
        var (_, first) = await RosterClient.GetAsync(roster, Pupils + "?pageSize=10&filter[status]=enrolled");
        var cursor = first.GetProperty("nextCursor").GetString()!;
        var tampered = cursor[..^2] + (cursor[^2] == 'A' ? 'B' : 'A') + cursor[^1];

        foreach (var url in new[]
        {
            RosterClient.WithCursor(Pupils + "?pageSize=10&filter[status]=enrolled", tampered),
            RosterClient.WithCursor(Pupils + "?pageSize=10&filter[status]=enrolled&sort=-lastName", cursor),
            RosterClient.WithCursor(Pupils + "?pageSize=10&filter[status]=suspended", cursor),
            RosterClient.WithCursor("/api/v1/probe/archived-pupils?pageSize=10&filter[status]=enrolled", cursor),
            RosterClient.WithCursor(Pupils + "?pageSize=10&filter[status]=enrolled", "not-a-cursor"),
        })
        {
            var (status, body) = await RosterClient.GetAsync(roster, url);

            status.ShouldBe(HttpStatusCode.BadRequest, url);
            body.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
            body.GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("cursor");
        }

        var (ok, second) = await RosterClient.GetAsync(roster, RosterClient.WithCursor(Pupils + "?pageSize=10&filter[status]=enrolled", cursor));
        ok.ShouldBe(HttpStatusCode.OK);
        RosterClient.Ids(second).Intersect(RosterClient.Ids(first)).ShouldBeEmpty();
    }

    [Fact]
    [Trait("TestCase", "TC-API-002")]
    public async Task Another_tenant_presenting_a_cursor_still_sees_only_its_own_rows()
    {
        var (_, first) = await RosterClient.GetAsync(roster, Pupils + "?pageSize=5");

        var (status, page) = await RosterClient.GetAsync(
            roster, RosterClient.WithCursor(Pupils + "?pageSize=5", first.GetProperty("nextCursor").GetString()!), RosterFixture.TenantB);

        status.ShouldBe(HttpStatusCode.OK);
        RosterClient.Ids(page).Intersect(roster.Seeded.Select(p => p.Id)).ShouldBeEmpty();
    }

    /// <summary>The ground truth: the whole list in one ordered query, as PostgreSQL orders it.</summary>
    private async Task<List<Guid>> DatabaseOrderAsync(string sort)
    {
        await using var scope = roster.ScopeFor(RosterFixture.TenantA);
        IQueryable<RosterPupil> pupils = scope.ServiceProvider.GetRequiredService<RosterDbContext>().Pupils.AsNoTracking();
        IOrderedQueryable<RosterPupil> ordered = sort switch
        {
            "lastName" => pupils.OrderBy(p => p.LastName),
            "-enrolledAt" => pupils.OrderByDescending(p => p.EnrolledAt),
            "nickname" => pupils.OrderBy(p => p.Nickname),
            "-nickname" => pupils.OrderByDescending(p => p.Nickname),
            "-balance,firstName" => pupils.OrderByDescending(p => p.Balance).ThenBy(p => p.FirstName),
            "status,-dateOfBirth,lastName" => pupils.OrderBy(p => p.Status).ThenByDescending(p => p.DateOfBirth).ThenBy(p => p.LastName),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null),
        };
        return await ordered.ThenBy(p => p.Id).Select(p => p.Id).ToListAsync(Ct);
    }

    /// <summary>Counts the commands EF Core sends for the roster's pupils table while it is alive.</summary>
    private sealed class CommandCounter : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>, IDisposable
    {
        private readonly IDisposable _all;
        private readonly List<IDisposable> _subscriptions = [];
        private int _pupils;

        public CommandCounter() => _all = DiagnosticListener.AllListeners.Subscribe(this);

        public int Pupils => Volatile.Read(ref _pupils);

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == DbLoggerCategory.Name)
            {
                lock (_subscriptions)
                {
                    _subscriptions.Add(value.Subscribe(this));
                }
            }
        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (value.Key == RelationalEventId.CommandExecuted.Name
                && value.Value is CommandExecutedEventData { Context: RosterDbContext } data
                && data.Command.CommandText.Contains("pupils", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref _pupils);
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void Dispose()
        {
            _all.Dispose();
            lock (_subscriptions)
            {
                _subscriptions.ForEach(s => s.Dispose());
            }
        }
    }
}
