using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Nibras.BuildingBlocks.Web;

/// <summary>
/// The strong ETag of document 22 §4: <c>"&lt;xmin&gt;-&lt;8-hex hash of id&gt;"</c>. The row version says which version;
/// the hash stops the tag of one row from matching another row that happens to share the version.
/// </summary>
public static class ETags
{
    public static string For(uint rowVersion, Guid id)
    {
        Span<byte> bytes = stackalloc byte[16];
        id.TryWriteBytes(bytes, bigEndian: true, out _);
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(bytes, hash);
        return string.Create(CultureInfo.InvariantCulture, $"\"{rowVersion}-{Convert.ToHexStringLower(hash[..4])}\"");
    }

    /// <summary>True when an <c>If-None-Match</c> value names this tag, or is <c>*</c>. Weak tags never match (§4: never weak).</summary>
    public static bool Matches(string? ifNoneMatch, string etag)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatch) || !EntityTagHeaderValue.TryParseList([ifNoneMatch], out var tags))
        {
            return false;
        }

        var current = new EntityTagHeaderValue(etag);
        return tags.Any(t => t.Equals(EntityTagHeaderValue.Any) || t.Compare(current, useStrongComparison: true));
    }
}

/// <summary>A single-resource read with its ETag. The <see cref="ETagFilter"/> turns it into 304 when the client already has it.</summary>
public interface IVersionedResult : IResult
{
    string ETag { get; }
}

/// <summary>200 with the resource and its <c>ETag</c> header.</summary>
public sealed class VersionedResult<T>(T value, string etag) : IVersionedResult, IValueHttpResult, IValueHttpResult<T>, IStatusCodeHttpResult
{
    public T Value { get; } = value;

    object? IValueHttpResult.Value => Value;

    public string ETag { get; } = etag;

    public int? StatusCode => StatusCodes.Status200OK;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        httpContext.Response.Headers.ETag = ETag;
        return TypedResults.Ok(Value).ExecuteAsync(httpContext);
    }
}

public static partial class NibrasResults
{
    /// <summary>A single-resource read, tagged from the aggregate's <c>xmin</c> and id.</summary>
    public static VersionedResult<T> Versioned<T>(T value, Guid id, uint rowVersion) => new(value, ETags.For(rowVersion, id));
}

/// <summary>
/// Answers a <c>GET</c> or <c>HEAD</c> whose <c>If-None-Match</c> names the current tag with 304, the same
/// <c>ETag</c> and no body (REQ-PERF-031). Every endpoint of <c>MapNibrasApi</c> carries it.
/// </summary>
public sealed class ETagFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        var result = await next(context).ConfigureAwait(false);
        var request = context.HttpContext.Request;
        if (result is IVersionedResult versioned
            && (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
            && ETags.Matches(request.Headers.IfNoneMatch, versioned.ETag))
        {
            return new NotModifiedResult(versioned.ETag);
        }

        return result;
    }

    private sealed class NotModifiedResult(string etag) : IResult, IStatusCodeHttpResult
    {
        public int? StatusCode => StatusCodes.Status304NotModified;

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
            httpContext.Response.Headers.ETag = etag;
            return Task.CompletedTask;
        }
    }
}
