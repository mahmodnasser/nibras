using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Nibras.BuildingBlocks.Web;

/// <summary>
/// The <c>If-Match</c> header of an update (document 22 §4). A handler takes it as a parameter, checks it against
/// the aggregate it loaded, and saves only over the version it names (REQ-API-014). The <see cref="IfMatchFilter"/>
/// has already refused a missing or malformed header before the handler runs (REQ-API-015).
/// </summary>
public sealed class IfMatch
{
    private static readonly IfMatch Missing = new([], isAny: false, isPresent: false);

    private readonly IReadOnlyList<string> _tags;

    private IfMatch(IReadOnlyList<string> tags, bool isAny, bool isPresent)
    {
        _tags = tags;
        IsAny = isAny;
        IsPresent = isPresent;
    }

    /// <summary><c>If-Match: *</c>, accepted only on <c>DELETE</c>: delete whatever version exists.</summary>
    public bool IsAny { get; }

    public bool IsPresent { get; }

    /// <summary>
    /// The row version the client read, when the header names exactly one tag and that tag belongs to
    /// <paramref name="id"/>. A tag of another row never yields a version, so it can never match.
    /// </summary>
    public bool TryGetRowVersion(Guid id, out uint rowVersion)
    {
        rowVersion = 0;
        if (_tags is not [var tag])
        {
            return false;
        }

        var dash = tag.IndexOf('-', StringComparison.Ordinal);
        return dash > 1
            && uint.TryParse(tag.AsSpan(1, dash - 1), NumberStyles.None, CultureInfo.InvariantCulture, out rowVersion)
            && string.Equals(ETags.For(rowVersion, id), tag, StringComparison.Ordinal);
    }

    /// <summary>True when the header is <c>*</c> or names the current tag of the aggregate.</summary>
    public bool Matches(Guid id, uint currentRowVersion)
    {
        if (IsAny)
        {
            return true;
        }

        var current = ETags.For(currentRowVersion, id);
        return _tags.Any(t => string.Equals(t, current, StringComparison.Ordinal));
    }

    /// <summary>Minimal API binding: the parameter is always bound; the filter has judged the header already.</summary>
    public static ValueTask<IfMatch?> BindAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult<IfMatch?>(Parse(context.Request.Headers.IfMatch, out var parsed) == IfMatchProblem.None ? parsed : Missing);
    }

    internal static IfMatchProblem Parse(StringValues header, out IfMatch parsed)
    {
        parsed = Missing;
        if (StringValues.IsNullOrEmpty(header) || header.All(string.IsNullOrWhiteSpace))
        {
            return IfMatchProblem.Missing;
        }

        if (!EntityTagHeaderValue.TryParseStrictList(header, out var tags) || tags.Count == 0)
        {
            return IfMatchProblem.Malformed;
        }

        if (tags.Any(t => t.Equals(EntityTagHeaderValue.Any)))
        {
            if (tags.Count != 1)
            {
                return IfMatchProblem.Malformed;
            }

            parsed = new IfMatch([], isAny: true, isPresent: true);
            return IfMatchProblem.None;
        }

        if (tags.Any(t => t.IsWeak))
        {
            return IfMatchProblem.Weak; // §4: tags are never weak, so a weak one cannot name a version
        }

        parsed = new IfMatch([.. tags.Select(t => t.Tag.Value!)], isAny: false, isPresent: true);
        return IfMatchProblem.None;
    }
}

internal enum IfMatchProblem
{
    None,
    Missing,
    Malformed,
    Weak,
}

/// <summary>Marks an update endpoint that is not an aggregate root's, so the <see cref="IfMatchFilter"/> leaves it alone.</summary>
public sealed class WithoutIfMatchMetadata
{
    internal static readonly WithoutIfMatchMetadata Instance = new();

    private WithoutIfMatchMetadata()
    {
    }
}

/// <summary>
/// Refuses a <c>PUT</c>, <c>PATCH</c> or <c>DELETE</c> without a usable <c>If-Match</c> with 400
/// <c>_VALIDATION_FAILED</c> and the field <c>If-Match</c>, before the handler runs, so nothing is overwritten
/// silently (REQ-API-015). <c>If-Match: *</c> passes only on <c>DELETE</c>. Every endpoint of
/// <c>MapNibrasApi</c> carries it unless it is marked <see cref="ConcurrencyConventions.WithoutIfMatch{TBuilder}"/>.
/// </summary>
internal static class IfMatchFilter
{
    internal static EndpointFilterDelegate Create(EndpointBuilder endpoint, EndpointFilterFactoryContext _, EndpointFilterDelegate next)
    {
        var metadata = endpoint.Metadata;
        var methods = metadata.OfType<IHttpMethodMetadata>().SelectMany(m => m.HttpMethods).ToArray();
        var guarded = methods.Length > 0
            && methods.All(m => HttpMethods.IsPut(m) || HttpMethods.IsPatch(m) || HttpMethods.IsDelete(m))
            && !metadata.OfType<WithoutIfMatchMetadata>().Any();
        if (!guarded)
        {
            return next;
        }

        return async context =>
        {
            var request = context.HttpContext.Request;
            var problem = IfMatch.Parse(request.Headers.IfMatch, out var parsed);
            var reason = problem switch
            {
                IfMatchProblem.Missing => "required",
                IfMatchProblem.Malformed => "invalid",
                IfMatchProblem.Weak => "weakTag",
                _ when parsed.IsAny && !HttpMethods.IsDelete(request.Method) => "wildcardNotAllowed",
                _ => null,
            };
            return reason is null
                ? await next(context).ConfigureAwait(false)
                : RefusedIfMatch(reason);
        };
    }

    private static CrossCuttingProblemResult RefusedIfMatch(string reason) =>
        new(
            ErrorCatalog.ValidationFailed,
            reason == "required"
                ? "Send the ETag you last read in If-Match, so a change made since then is not overwritten."
                : "If-Match must hold the strong ETag you last read.",
            errors: [new FieldError(HeaderNames.IfMatch, reason)]);
}

public static class ConcurrencyConventions
{
    /// <summary>
    /// Exempts an update endpoint that changes no aggregate root (ending a session, clearing a draft) from the
    /// <c>If-Match</c> requirement. An aggregate root's update never carries it.
    /// </summary>
    public static TBuilder WithoutIfMatch<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(WithoutIfMatchMetadata.Instance);
        return builder;
    }
}

public static partial class NibrasResults
{
    /// <summary>
    /// 409 <c>_CONCURRENCY_CONFLICT</c> for an update sent with a stale tag: <c>params.currentEtag</c> tells the
    /// client what to re-read and, for a <c>PATCH</c>, <c>params.changedFields</c> says what changed since (document 22 §4).
    /// </summary>
    public static IResult ConcurrencyConflict(string currentEtag, IEnumerable<string>? changedFields = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentEtag);
        var parameters = new JsonObject { ["currentEtag"] = currentEtag };
        if (changedFields is not null)
        {
            parameters["changedFields"] = new JsonArray([.. changedFields.Select(f => JsonValue.Create(f))]);
        }

        return new CrossCuttingProblemResult(
            ErrorCatalog.ConcurrencyConflict,
            "The resource changed after you read it. Re-read it and apply your change again.",
            parameters);
    }
}
