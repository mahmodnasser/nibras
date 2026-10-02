using System.Buffers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Tenancy;
using StackExchange.Redis;

namespace Nibras.BuildingBlocks.Web;

/// <summary>How long a key is remembered from first sight (document 22 §5).</summary>
public static class IdempotencyLifetime
{
    /// <summary>A phone that lost signal on a school trip still replays the next morning.</summary>
    public static readonly TimeSpan Default = TimeSpan.FromHours(24);

    /// <summary>Payment gateways retry their callbacks for days.</summary>
    public static readonly TimeSpan PaymentCallback = TimeSpan.FromDays(7);
}

/// <summary>An endpoint whose <c>POST</c> must carry an <c>Idempotency-Key</c> (<c>x-nibras-idempotency: required</c>).</summary>
public sealed class IdempotencyRequiredMetadata(TimeSpan lifetime)
{
    public TimeSpan Lifetime { get; } = lifetime;
}

public static class IdempotencyConventions
{
    /// <summary>
    /// Requires an <c>Idempotency-Key</c> on this <c>POST</c>: payments and payment callbacks, submissions, attendance
    /// marks, request submissions, message sends, gate passes, bulk and job endpoints, and every <c>POST</c> the mobile
    /// outbox replays (document 22 §5). Any other <c>POST</c> honours a key when one is sent.
    /// </summary>
    public static TBuilder RequireIdempotencyKey<TBuilder>(this TBuilder builder, TimeSpan? lifetime = null)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        var value = lifetime ?? IdempotencyLifetime.Default;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero, nameof(lifetime));
        builder.WithMetadata(new IdempotencyRequiredMetadata(value));
        return builder;
    }

    /// <summary>
    /// The <c>redis-state</c> store for idempotency keys (no eviction, persisted; document 21 §2.2). A service with
    /// an endpoint that requires a key must call this, or its host refuses to build that endpoint.
    /// </summary>
    public static IServiceCollection AddNibrasIdempotency(this IServiceCollection services, string redisStateConnection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(redisStateConnection);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IIdempotencyStore>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisStateConnection);
            options.AbortOnConnectFail = false; // the host starts while redis-state is down; required endpoints answer 503
            options.BacklogPolicy = BacklogPolicy.FailFast; // a command during an outage fails now rather than after the timeout
            return new RedisIdempotencyStore(ConnectionMultiplexer.Connect(options));
        });
        return services;
    }
}

/// <summary>What the store holds for one key: the first request's fingerprint and, once it finished, its response.</summary>
internal sealed record IdempotencyEntry(
    string Fingerprint,
    bool Completed,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset ExpiresAt,
    string CorrelationId,
    int Status = 0,
    string? ContentType = null,
    string? Location = null,
    string? ETag = null,
    byte[]? Body = null,
    bool BodyOmitted = false);

/// <summary>The store failed (connection, timeout). Required endpoints fail closed, optional ones open.</summary>
internal sealed class IdempotencyStoreUnavailableException(string message, Exception inner) : Exception(message, inner);

/// <summary>Holds idempotency entries. Every write is conditional on the entry it replaces, so two requests never both win.</summary>
internal interface IIdempotencyStore
{
    /// <summary>Stores <paramref name="inFlight"/> if the key is free and returns null; otherwise returns what is there.</summary>
    Task<IdempotencyEntry?> TryBeginAsync(string key, IdempotencyEntry inFlight, TimeSpan lease);

    /// <summary>Replaces this request's in-flight entry with its response, kept until <see cref="IdempotencyEntry.ExpiresAt"/>.</summary>
    Task CompleteAsync(string key, IdempotencyEntry inFlight, IdempotencyEntry completed, TimeSpan timeToLive);

    /// <summary>Removes <paramref name="entry"/> if it is still the one stored: a failed request, or one past its lifetime.</summary>
    Task RemoveAsync(string key, IdempotencyEntry entry);
}

internal sealed class RedisIdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
{
    public async Task<IdempotencyEntry?> TryBeginAsync(string key, IdempotencyEntry inFlight, TimeSpan lease)
    {
        var database = redis.GetDatabase();
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var stored = await Guard(() => database.StringSetAsync(key, Serialize(inFlight), lease, When.NotExists)).ConfigureAwait(false);
            if (stored)
            {
                return null;
            }

            var existing = await Guard(() => database.StringGetAsync(key)).ConfigureAwait(false);
            if (existing.HasValue)
            {
                return Deserialize(existing);
            }

            // The entry expired between the two commands: try to take the key again.
        }

        throw new IdempotencyStoreUnavailableException("The idempotency key kept changing under three attempts.", new InvalidOperationException(key));
    }

    public Task CompleteAsync(string key, IdempotencyEntry inFlight, IdempotencyEntry completed, TimeSpan timeToLive) =>
        Guard(async () =>
        {
            var transaction = redis.GetDatabase().CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(key, Serialize(inFlight)));
            _ = transaction.StringSetAsync(key, Serialize(completed), timeToLive, When.Always);
            return await transaction.ExecuteAsync().ConfigureAwait(false);
        });

    // SER301 suggests DELEX (Redis 8.4+). WATCH and MULTI stay, because the Valkey fallback must run the same commands (document 21 §2.5).
#pragma warning disable SER301
    public Task RemoveAsync(string key, IdempotencyEntry entry) =>
        Guard(async () =>
        {
            var transaction = redis.GetDatabase().CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(key, Serialize(entry)));
            _ = transaction.KeyDeleteAsync(key);
            return await transaction.ExecuteAsync().ConfigureAwait(false);
        });
#pragma warning restore SER301

    private static async Task<T> Guard<T>(Func<Task<T>> command)
    {
        try
        {
            return await command().ConfigureAwait(false);
        }
        catch (Exception e) when (e is RedisException or TimeoutException)
        {
            throw new IdempotencyStoreUnavailableException("redis-state is unavailable.", e);
        }
    }

    private static RedisValue Serialize(IdempotencyEntry entry) =>
        JsonSerializer.SerializeToUtf8Bytes(entry, IdempotencyJsonContext.Default.IdempotencyEntry);

    private static IdempotencyEntry Deserialize(RedisValue value) =>
        JsonSerializer.Deserialize((byte[])value!, IdempotencyJsonContext.Default.IdempotencyEntry)
        ?? throw new JsonException("An idempotency entry was empty.");
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IdempotencyEntry))]
internal sealed partial class IdempotencyJsonContext : JsonSerializerContext;

/// <summary>
/// The <c>Idempotency-Key</c> contract of document 22 §5 (REQ-API-016). The first request with a key runs and its
/// response (a success or a 4xx Problem Details) is stored; the same key with the same body replays it without running
/// the handler again, a different body is refused, and a duplicate that arrives while the first still runs is told to
/// retry. Every <c>POST</c> of <c>MapNibrasApi</c> carries it.
/// </summary>
internal static partial class IdempotencyKeyFilter
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayedHeader = "Idempotency-Replayed";

    /// <summary>How long a request may run before its key is free again. A longer piece of work is a job (§6).</summary>
    internal static readonly TimeSpan InFlightLease = TimeSpan.FromMinutes(2);

    /// <summary>Larger responses are stored without their body; the replay carries the status and <c>Location</c> to re-fetch.</summary>
    internal const int MaxStoredBody = 64 * 1024;

    internal static EndpointFilterDelegate Create(EndpointBuilder endpoint, EndpointFilterFactoryContext factory, EndpointFilterDelegate next)
    {
        var metadata = endpoint.Metadata;
        var methods = metadata.OfType<IHttpMethodMetadata>().SelectMany(m => m.HttpMethods).ToArray();
        if (methods.Length == 0 || !methods.All(HttpMethods.IsPost))
        {
            return next;
        }

        var required = metadata.OfType<IdempotencyRequiredMetadata>().LastOrDefault();
        var store = factory.ApplicationServices.GetService<IIdempotencyStore>();
        if (store is null)
        {
            return required is null
                ? next
                : throw new InvalidOperationException(
                    "An endpoint requires an Idempotency-Key but no store is registered. Call AddNibrasIdempotency with the redis-state connection.");
        }

        var template = (endpoint as RouteEndpointBuilder)?.RoutePattern.RawText
            ?? factory.MethodInfo.DeclaringType?.FullName + "." + factory.MethodInfo.Name;
        var lifetime = required?.Lifetime ?? IdempotencyLifetime.Default;
        return context => InvokeAsync(context, next, store, required is not null, lifetime, template);
    }

    private static async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next, IIdempotencyStore store, bool required, TimeSpan lifetime, string template)
    {
        var http = context.HttpContext;
        var header = http.Request.Headers[HeaderName];
        if (header.Count == 0 || string.IsNullOrEmpty(header[0]))
        {
            return required ? RefusedKey("required", "This operation needs an Idempotency-Key: one UUID per logical operation.") : await next(context).ConfigureAwait(false);
        }

        var key = header.Count == 1 ? header[0]! : "";
        if (!KeyFormat().IsMatch(key))
        {
            return RefusedKey("invalid", "An Idempotency-Key is 1 to 64 characters of A-Z, a-z, 0-9, '_' and '-'.");
        }

        var clock = http.RequestServices.GetService<TimeProvider>() ?? TimeProvider.System;
        var now = clock.GetUtcNow();
        var storageKey = StorageKey(http, template, key);
        var fingerprint = await FingerprintAsync(http.Request).ConfigureAwait(false);
        var inFlight = new IdempotencyEntry(fingerprint, Completed: false, now, now + lifetime, CorrelationOf(http));

        IdempotencyEntry? existing;
        try
        {
            existing = await store.TryBeginAsync(storageKey, inFlight, InFlightLease).ConfigureAwait(false);
            if (existing is not null && existing.ExpiresAt <= now)
            {
                await store.RemoveAsync(storageKey, existing).ConfigureAwait(false); // past its lifetime: a new operation
                existing = await store.TryBeginAsync(storageKey, inFlight, InFlightLease).ConfigureAwait(false);
            }
        }
        catch (IdempotencyStoreUnavailableException e)
        {
            if (required)
            {
                // A duplicate payment is worse than a delayed one: nothing runs until the store is back.
                return new CrossCuttingProblemResult(
                    ErrorCatalog.DependencyUnavailable, "The operation cannot be protected against repetition right now. Retry shortly.", retryAfterSeconds: 5);
            }

            LogFailOpen(Logger(http), e, template);
            return await next(context).ConfigureAwait(false);
        }

        if (existing is not null)
        {
            if (!string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
            {
                return new CrossCuttingProblemResult(
                    ErrorCatalog.ValidationFailed,
                    "This Idempotency-Key was already used for a different request. Use a new key for a new operation.",
                    new JsonObject { ["reason"] = "fingerprintMismatch" },
                    [new FieldError(HeaderName, "fingerprintMismatch")]);
            }

            if (!existing.Completed)
            {
                return new CrossCuttingProblemResult(
                    ErrorCatalog.ConcurrencyConflict, "The first request with this Idempotency-Key is still running. Retry shortly.", retryAfterSeconds: 2);
            }

            var logger = Logger(http);
            if (logger.IsEnabled(LogLevel.Information))
            {
                LogReplay(logger, template, existing.CorrelationId, inFlight.CorrelationId);
            }

            return new ReplayResult(existing);
        }

        return await RunAndStoreAsync(context, next, store, storageKey, inFlight, clock).ConfigureAwait(false);
    }

    private static async ValueTask<object?> RunAndStoreAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next, IIdempotencyStore store, string storageKey, IdempotencyEntry inFlight, TimeProvider clock)
    {
        var http = context.HttpContext;
        var response = http.Response;
        var completed = false;
        try
        {
            var result = await next(context).ConfigureAwait(false);

            // Run the result into a buffer so the bytes the client receives are exactly the bytes stored.
            var original = response.Body;
            using var buffer = new MemoryStream();
            response.Body = buffer;
            try
            {
                await AsResult(result).ExecuteAsync(http).ConfigureAwait(false);
            }
            finally
            {
                response.Body = original;
            }

            if (response.StatusCode < StatusCodes.Status500InternalServerError)
            {
                var omitted = buffer.Length > MaxStoredBody;
                var entry = inFlight with
                {
                    Completed = true,
                    Status = response.StatusCode,
                    ContentType = response.ContentType,
                    Location = response.Headers.Location.ToString() is { Length: > 0 } location ? location : null,
                    ETag = response.Headers.ETag.ToString() is { Length: > 0 } etag ? etag : null,
                    Body = omitted ? null : buffer.ToArray(),
                    BodyOmitted = omitted,
                };
                var timeToLive = inFlight.ExpiresAt - clock.GetUtcNow();
                try
                {
                    await store.CompleteAsync(storageKey, inFlight, entry, timeToLive > TimeSpan.Zero ? timeToLive : TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }
                catch (IdempotencyStoreUnavailableException e)
                {
                    // The work is committed: the client gets its real answer, not a 500 that invites a duplicate.
                    LogCompleteFailed(Logger(http), e);
                }

                completed = true;
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(original, http.RequestAborted).ConfigureAwait(false);
            return Results.Empty;
        }
        finally
        {
            if (!completed)
            {
                await ReleaseAsync(http, store, storageKey, inFlight).ConfigureAwait(false);
            }
        }
    }

    /// <summary>A failed or crashed request frees its key, so the client's retry runs the operation rather than waiting out the lease.</summary>
    private static async Task ReleaseAsync(HttpContext http, IIdempotencyStore store, string storageKey, IdempotencyEntry inFlight)
    {
        try
        {
            await store.RemoveAsync(storageKey, inFlight).ConfigureAwait(false);
        }
        catch (IdempotencyStoreUnavailableException e)
        {
            LogReleaseFailed(Logger(http), e); // the lease expires on its own
        }
    }

    private static IResult AsResult(object? result) => result switch
    {
        IResult r => r,
        null => Results.Empty,
        string text => TypedResults.Text(text),
        _ => TypedResults.Json(result),
    };

    private static CrossCuttingProblemResult RefusedKey(string reason, string detail) =>
        new(ErrorCatalog.ValidationFailed, detail, errors: [new FieldError(HeaderName, reason)]);

    /// <summary>
    /// <c>state:idem:{tenant}:{service}:{sha256(scope)}</c>, inside the service's ACL pattern of document 21 §2.2.
    /// The scope is the tenant, the caller, the method, the route template and the key: the same key from another
    /// user or on another endpoint is another operation.
    /// </summary>
    internal static string StorageKey(HttpContext http, string template, string key)
    {
        var tenant = http.RequestServices.GetService<ITenantContext>()?.Tenant?.Value.ToString("D") ?? "platform";
        var service = http.RequestServices.GetRequiredService<IOptions<NibrasWebOptions>>().Value.ApiSegment;
        var caller = http.User.FindFirstValue("sub") ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var scope = string.Join('\n', tenant, caller, http.Request.Method, template, key);
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(scope)));
        return $"state:idem:{tenant}:{service}:{hash}";
    }

    /// <summary>SHA-256 of the canonical body (JSON with its members sorted, or the raw bytes) plus the <c>If-Match</c> header.</summary>
    internal static async Task<string> FingerprintAsync(HttpRequest request)
    {
        byte[] body = [];
        if (request.ContentLength is not 0)
        {
            if (!request.Body.CanSeek)
            {
                throw new InvalidOperationException("The request body was not buffered for its Idempotency-Key. Call UseNibrasWeb before the endpoints.");
            }

            request.Body.Position = 0;
            using var copy = new MemoryStream();
            await request.Body.CopyToAsync(copy, request.HttpContext.RequestAborted).ConfigureAwait(false);
            request.Body.Position = 0;
            body = Canonical(copy.ToArray());
        }

        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha.AppendData(body);
        sha.AppendData("\n"u8);
        sha.AppendData(Encoding.UTF8.GetBytes(request.Headers.IfMatch.ToString()));
        return Convert.ToHexStringLower(sha.GetHashAndReset());
    }

    /// <summary>The same JSON document always gives the same bytes, whatever the member order or white space the client sent.</summary>
    internal static byte[] Canonical(byte[] body)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(body);
        }
        catch (JsonException)
        {
            return body;
        }

        var output = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(output))
        {
            WriteSorted(writer, node);
        }

        return output.WrittenSpan.ToArray();
    }

    private static void WriteSorted(Utf8JsonWriter writer, JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                writer.WriteStartObject();
                foreach (var (name, value) in obj.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(name);
                    WriteSorted(writer, value);
                }

                writer.WriteEndObject();
                break;
            case JsonArray array:
                writer.WriteStartArray();
                foreach (var item in array)
                {
                    WriteSorted(writer, item);
                }

                writer.WriteEndArray();
                break;
            case null:
                writer.WriteNullValue();
                break;
            default:
                node.WriteTo(writer);
                break;
        }
    }

    private static string CorrelationOf(HttpContext http) =>
        http.Items[CorrelationIdMiddleware.ItemKey] as string ?? http.Response.Headers[TelemetryConventions.CorrelationHeader].ToString();

    private static ILogger Logger(HttpContext http) =>
        http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Nibras.BuildingBlocks.Web.Idempotency");

    [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyFormat();

    [LoggerMessage(EventId = 5100, Level = LogLevel.Information, Message = "Idempotency replay on {Route}: original correlation {OriginalCorrelationId}, replay correlation {ReplayCorrelationId}")]
    private static partial void LogReplay(ILogger logger, string route, string originalCorrelationId, string replayCorrelationId);

    [LoggerMessage(EventId = 5101, Level = LogLevel.Warning, Message = "redis-state unavailable; the optional Idempotency-Key on {Route} is not honoured")]
    private static partial void LogFailOpen(ILogger logger, Exception exception, string route);

    [LoggerMessage(EventId = 5103, Level = LogLevel.Error, Message = "A committed response could not be stored for its Idempotency-Key; a retry within the lease is told to wait, after it the operation runs again")]
    private static partial void LogCompleteFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5102, Level = LogLevel.Warning, Message = "An in-flight idempotency entry could not be released; its lease expires on its own")]
    private static partial void LogReleaseFailed(ILogger logger, Exception exception);

    /// <summary>
    /// The stored response again: 200 for an original success, the original status for a 4xx Problem Details,
    /// with the original body verbatim and <c>Idempotency-Replayed: true</c>.
    /// </summary>
    private sealed class ReplayResult(IdempotencyEntry entry) : IResult, IStatusCodeHttpResult
    {
        public int? StatusCode => entry.Status is >= 200 and < 300 ? StatusCodes.Status200OK : entry.Status;

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var response = httpContext.Response;
            response.StatusCode = StatusCode!.Value;
            response.Headers[ReplayedHeader] = "true";
            response.Headers.CacheControl = "no-store";
            if (entry.ContentType is not null)
            {
                response.ContentType = entry.ContentType;
            }

            if (entry.Location is not null)
            {
                response.Headers[HeaderNames.Location] = entry.Location;
            }

            if (entry.ETag is not null)
            {
                response.Headers.ETag = entry.ETag;
            }

            if (entry.Body is { Length: > 0 } body)
            {
                await response.Body.WriteAsync(body, httpContext.RequestAborted).ConfigureAwait(false);
            }
        }
    }
}
