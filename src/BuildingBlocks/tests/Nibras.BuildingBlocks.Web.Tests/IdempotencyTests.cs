using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Domain;
using Shouldly;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>A clock the test moves by hand.</summary>
public sealed class FakeClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

/// <summary>One Redis 8.10.2 container standing in for <c>redis-state</c>.</summary>
public sealed class RedisState : IAsyncLifetime
{
    public RedisContainer Container { get; } = new RedisBuilder("redis:8.10.2").Build();

    public string ConnectionString => Container.GetConnectionString();

    public async ValueTask InitializeAsync() => await Container.StartAsync();

    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}

/// <summary>A probe host with the idempotency store, a clock the test moves, and endpoints that count their runs.</summary>
internal sealed class IdempotencyProbe
{
    private int _marks;
    private int _callbacks;
    private int _notes;
    private int _failures;

    public FakeClock Clock { get; } = new(new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero));

    /// <summary>Holds the slow endpoint open until the test releases it.</summary>
    public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Marks => Volatile.Read(ref _marks);

    public int Callbacks => Volatile.Read(ref _callbacks);

    public int Notes => Volatile.Read(ref _notes);

    public int Failures => Volatile.Read(ref _failures);

    public Task<ProbeApi> StartAsync(string redisState) => ProbeApi.StartAsync(
        configure: builder =>
        {
            builder.Services.AddSingleton<TimeProvider>(Clock);
            builder.Services.AddNibrasIdempotency(redisState + ",connectTimeout=1000,asyncTimeout=1000");
        },
        extraRoutes: app =>
        {
            var api = app.MapNibrasApi(1);
            api.MapPost("/sections/{id:guid}/attendance-marks", (Guid id, CreateStudent body) =>
            {
                var run = Interlocked.Increment(ref _marks);
                return TypedResults.Created(
                    $"/api/v1/probe/sections/{id}/attendance-marks/{run}",
                    ProbeApi.Student(ProbeApi.StudentId) with { Name = $"{body.Name} run {run}" });
            }).RequireIdempotencyKey();
            api.MapPost("/payment-callbacks", (CreateStudent body) =>
            {
                Interlocked.Increment(ref _callbacks);
                return TypedResults.Ok(ProbeApi.Student(ProbeApi.StudentId) with { Name = body.Name ?? "" });
            }).RequireIdempotencyKey(IdempotencyLifetime.PaymentCallback);
            api.MapPost("/notes", (CreateStudent body) =>
            {
                Interlocked.Increment(ref _notes);
                return TypedResults.Created("/api/v1/probe/notes/1", ProbeApi.Student(ProbeApi.StudentId));
            });
            api.MapPost("/sessions/{id:guid}/close", async (Guid id, CancellationToken ct) =>
            {
                Entered.TrySetResult();
                await Gate.Task.WaitAsync(ct);
                return TypedResults.Ok(ProbeApi.Student(id));
            }).RequireIdempotencyKey();
            api.MapPost("/sessions/{id:guid}/lock-attempts", (Guid id) =>
                Result.Failure(new Error("PROBE_SESSION_LOCKED", $"Session {id} is locked.")).ToProblem()).RequireIdempotencyKey();
            api.MapPost("/flaky-imports", IResult () =>
            {
                if (Interlocked.Increment(ref _failures) == 1)
                {
                    throw new InvalidOperationException("The first attempt fails.");
                }

                return TypedResults.Ok(ProbeApi.Student(ProbeApi.StudentId));
            }).RequireIdempotencyKey();
        });
}

/// <summary>TC-API-022 to TC-API-025 (REQ-API-016): the <c>Idempotency-Key</c> contract of document 22 §5 against Redis.</summary>
public sealed class IdempotencyTests(RedisState redis) : IClassFixture<RedisState>
{
    private const string Body = """{"studentNumber":"S-0001","name":"Layla","dateOfBirth":"2012-03-04","status":"enrolled","enrolledAt":null}""";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string Path(Guid section) => $"/api/v1/probe/sections/{section}/attendance-marks";

    private static HttpRequestMessage Post(string path, string? key, string body = Body, Guid? tenant = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(path, UriKind.Relative))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Nibras-Tenant-Id", (tenant ?? ProbeApi.TenantHeaderValue).ToString("D"));
        if (key is not null)
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        }

        return request;
    }

    private static async Task<(HttpResponseMessage Response, string Text)> SendAsync(HttpClient client, HttpRequestMessage request)
    {
        using (request)
        {
            var response = await client.SendAsync(request, Ct);
            return (response, await response.Content.ReadAsStringAsync(Ct));
        }
    }

    private static bool Replayed(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Idempotency-Replayed", out var values) && values.Single() == "true";

    private static JsonElement Json(string text) => JsonDocument.Parse(text).RootElement.Clone();

    [Fact]
    [Trait("TestCase", "TC-API-022")]
    public async Task The_same_key_and_body_23_hours_later_replays_the_original_verbatim_and_runs_once()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = Guid.CreateVersion7().ToString("D");
        var section = Guid.CreateVersion7();

        var (first, firstText) = await SendAsync(client, Post(Path(section), key));
        probe.Clock.Now += TimeSpan.FromHours(23);
        var (again, againText) = await SendAsync(client, Post(Path(section), key));

        first.StatusCode.ShouldBe(HttpStatusCode.Created);
        Replayed(first).ShouldBeFalse();
        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        Replayed(again).ShouldBeTrue();
        againText.ShouldBe(firstText);
        again.Headers.Location.ShouldBe(first.Headers.Location);
        again.Content.Headers.ContentType!.MediaType.ShouldBe("application/json");
        probe.Marks.ShouldBe(1);
    }

    [Fact]
    [Trait("TestCase", "TC-API-022")]
    public async Task The_key_is_kept_in_redis_state_for_24_hours_from_first_sight_and_forgotten_after()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = Guid.CreateVersion7().ToString("D");
        var section = Guid.CreateVersion7();

        _ = await SendAsync(client, Post(Path(section), key));
        await using var connection = await ConnectionMultiplexer.ConnectAsync(redis.ConnectionString);
        var server = connection.GetServer(connection.GetEndPoints()[0]);
        var stored = server.Keys(pattern: $"state:idem:{ProbeApi.TenantHeaderValue:D}:probe:*").ToArray();
        var ttls = await Task.WhenAll(stored.Select(k => connection.GetDatabase().KeyTimeToLiveAsync(k)));

        ttls.ShouldContain(t => t > TimeSpan.FromHours(23.9) && t <= TimeSpan.FromHours(24));

        probe.Clock.Now += TimeSpan.FromHours(24) + TimeSpan.FromMinutes(1);
        var (later, _) = await SendAsync(client, Post(Path(section), key));
        later.StatusCode.ShouldBe(HttpStatusCode.Created);
        Replayed(later).ShouldBeFalse();
        probe.Marks.ShouldBe(2);
    }

    [Fact]
    [Trait("TestCase", "TC-API-022")]
    public async Task A_payment_callback_key_still_replays_after_6_days()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = "pay_" + Guid.CreateVersion7().ToString("N");

        var (first, firstText) = await SendAsync(client, Post("/api/v1/probe/payment-callbacks", key));
        probe.Clock.Now += TimeSpan.FromDays(6);
        var (again, againText) = await SendAsync(client, Post("/api/v1/probe/payment-callbacks", key));

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        Replayed(again).ShouldBeTrue();
        againText.ShouldBe(firstText);
        probe.Callbacks.ShouldBe(1);
    }

    [Fact]
    [Trait("TestCase", "TC-API-022")]
    public async Task The_same_body_with_its_members_reordered_and_spaced_is_the_same_request()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = Guid.CreateVersion7().ToString("D");
        var section = Guid.CreateVersion7();
        const string reordered = """
            { "name": "Layla", "status": "enrolled",
              "studentNumber": "S-0001", "enrolledAt": null, "dateOfBirth": "2012-03-04" }
            """;

        _ = await SendAsync(client, Post(Path(section), key));
        var (again, _) = await SendAsync(client, Post(Path(section), key, reordered));

        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        Replayed(again).ShouldBeTrue();
        probe.Marks.ShouldBe(1);
    }

    [Fact]
    [Trait("TestCase", "TC-API-022")]
    public async Task A_key_is_scoped_to_its_tenant_and_endpoint()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = Guid.CreateVersion7().ToString("D");
        var section = Guid.CreateVersion7();

        _ = await SendAsync(client, Post(Path(section), key));
        var (otherTenant, _) = await SendAsync(client, Post(Path(section), key, tenant: Guid.CreateVersion7()));
        var (otherEndpoint, _) = await SendAsync(client, Post("/api/v1/probe/notes", key));

        otherTenant.StatusCode.ShouldBe(HttpStatusCode.Created);
        Replayed(otherTenant).ShouldBeFalse();
        otherEndpoint.StatusCode.ShouldBe(HttpStatusCode.Created);
        probe.Marks.ShouldBe(2);
        probe.Notes.ShouldBe(1);
    }

    [Fact]
    [Trait("TestCase", "TC-API-022")]
    public async Task A_problem_details_answer_is_replayed_with_its_status_and_a_server_failure_is_not_kept()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var session = Guid.CreateVersion7();
        var key = Guid.CreateVersion7().ToString("D");
        var importKey = Guid.CreateVersion7().ToString("D");

        var (locked, lockedText) = await SendAsync(client, Post($"/api/v1/probe/sessions/{session}/lock-attempts", key, "{}"));
        var (lockedAgain, lockedAgainText) = await SendAsync(client, Post($"/api/v1/probe/sessions/{session}/lock-attempts", key, "{}"));
        var (failed, _) = await SendAsync(client, Post("/api/v1/probe/flaky-imports", importKey, "{}"));
        var (retried, _) = await SendAsync(client, Post("/api/v1/probe/flaky-imports", importKey, "{}"));

        locked.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        lockedAgain.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        Replayed(lockedAgain).ShouldBeTrue();
        lockedAgainText.ShouldBe(lockedText);
        lockedAgain.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        failed.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        retried.StatusCode.ShouldBe(HttpStatusCode.OK);
        Replayed(retried).ShouldBeFalse();
        probe.Failures.ShouldBe(2);
    }

    [Fact]
    [Trait("TestCase", "TC-API-023")]
    public async Task The_same_key_with_another_body_is_400_and_the_stored_original_is_unchanged()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = Guid.CreateVersion7().ToString("D");
        var section = Guid.CreateVersion7();

        var (first, firstText) = await SendAsync(client, Post(Path(section), key));
        var (mismatch, mismatchText) = await SendAsync(client, Post(Path(section), key, Body.Replace("Layla", "Huda", StringComparison.Ordinal)));
        var (again, againText) = await SendAsync(client, Post(Path(section), key));

        mismatch.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = Json(mismatchText);
        problem.GetProperty("code").GetString().ShouldBe("PROBE_VALIDATION_FAILED");
        problem.GetProperty("params").GetProperty("reason").GetString().ShouldBe("fingerprintMismatch");
        problem.GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("Idempotency-Key");
        Replayed(again).ShouldBeTrue();
        againText.ShouldBe(firstText);
        first.StatusCode.ShouldBe(HttpStatusCode.Created);
        probe.Marks.ShouldBe(1);
    }

    [Fact]
    [Trait("TestCase", "TC-API-024")]
    public async Task A_duplicate_while_the_first_still_runs_is_409_with_retry_after_2_then_gets_the_replay()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var key = Guid.CreateVersion7().ToString("D");
        var path = $"/api/v1/probe/sessions/{Guid.CreateVersion7()}/close";

        var running = SendAsync(client, Post(path, key, "{}"));
        await probe.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);
        var (duplicate, duplicateText) = await SendAsync(client, Post(path, key, "{}"));
        probe.Gate.SetResult();
        var (first, firstText) = await running;
        var (retry, retryText) = await SendAsync(client, Post(path, key, "{}"));

        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        Json(duplicateText).GetProperty("code").GetString().ShouldBe("PROBE_CONCURRENCY_CONFLICT");
        duplicate.Headers.RetryAfter!.Delta.ShouldBe(TimeSpan.FromSeconds(2));
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        retry.StatusCode.ShouldBe(HttpStatusCode.OK);
        Replayed(retry).ShouldBeTrue();
        retryText.ShouldBe(firstText);
    }

    [Fact]
    [Trait("TestCase", "TC-API-024")]
    public async Task A_required_key_that_is_missing_or_malformed_is_400_before_anything_runs_and_1_to_64_characters_pass()
    {
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(redis.ConnectionString);
        using var client = api.Client();
        var section = Guid.CreateVersion7();

        var (missing, missingText) = await SendAsync(client, Post(Path(section), key: null));
        var (malformed, malformedText) = await SendAsync(client, Post(Path(section), key: "not a key!"));
        var (tooLong, _) = await SendAsync(client, Post(Path(section), key: new string('a', 65)));
        var (longest, _) = await SendAsync(client, Post(Path(section), key: new string('a', 64)));
        var (shortest, _) = await SendAsync(client, Post(Path(section), key: "b"));

        missing.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        Json(missingText).GetProperty("errors")[0].GetProperty("code").GetString().ShouldBe("required");
        malformed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        Json(malformedText).GetProperty("errors")[0].GetProperty("field").GetString().ShouldBe("Idempotency-Key");
        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        longest.StatusCode.ShouldBe(HttpStatusCode.Created);
        shortest.StatusCode.ShouldBe(HttpStatusCode.Created);
        probe.Marks.ShouldBe(2); // only the two well-formed keys ran
    }

    [Fact]
    [Trait("TestCase", "TC-API-025")]
    public async Task With_redis_state_stopped_a_required_endpoint_is_503_and_writes_nothing_while_an_optional_one_proceeds()
    {
        await using var outage = new RedisState();
        await outage.InitializeAsync();
        var probe = new IdempotencyProbe();
        await using var api = await probe.StartAsync(outage.ConnectionString);
        using var client = api.Client();
        var (before, _) = await SendAsync(client, Post(Path(Guid.CreateVersion7()), Guid.CreateVersion7().ToString("D")));

        await outage.Container.StopAsync(Ct);
        var (required, requiredText) = await SendAsync(client, Post(Path(Guid.CreateVersion7()), Guid.CreateVersion7().ToString("D")));
        var (optional, _) = await SendAsync(client, Post("/api/v1/probe/notes", Guid.CreateVersion7().ToString("D")));

        before.StatusCode.ShouldBe(HttpStatusCode.Created);
        required.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        Json(requiredText).GetProperty("code").GetString().ShouldBe("PROBE_DEPENDENCY_UNAVAILABLE");
        required.Headers.RetryAfter!.Delta.ShouldBe(TimeSpan.FromSeconds(5));
        probe.Marks.ShouldBe(1);
        optional.StatusCode.ShouldBe(HttpStatusCode.Created);
        probe.Notes.ShouldBe(1);
    }

    [Fact]
    [Trait("TestCase", "TC-API-025")]
    public async Task A_host_with_a_required_key_and_no_store_refuses_to_start()
    {
        var error = await Should.ThrowAsync<InvalidOperationException>(() => ProbeApi.StartAsync(
            extraRoutes: app => app.MapNibrasApi(1).MapPost("/gate-passes", () => TypedResults.Ok()).RequireIdempotencyKey()));

        error.Message.ShouldContain("AddNibrasIdempotency");
    }
}
