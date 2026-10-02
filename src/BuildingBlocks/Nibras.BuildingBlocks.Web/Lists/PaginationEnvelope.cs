using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nibras.BuildingBlocks.Application.Lists;

namespace Nibras.BuildingBlocks.Web.Lists;

/// <summary>
/// The one keyset envelope of document 22 §2.1, with every member always present and no total count. The
/// cursors are written as <c>null</c> rather than omitted, because a client reads their absence as "unknown".
/// </summary>
public sealed record PaginationEnvelope<T>(
    IReadOnlyList<T> Items,
    int PageSize,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] string? NextCursor,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] string? PreviousCursor,
    bool HasMore,
    string Sort,
    string FilterHash);

public static class PaginationEnvelope
{
    /// <summary>
    /// Serves a keyset list: parses the grammar against the declared fields, resolves the cursor, runs the
    /// query and answers with the envelope, or with <c>_VALIDATION_FAILED</c> naming every refused parameter.
    /// </summary>
    public static async Task<IResult> KeysetAsync<TEntity, T>(
        HttpContext context,
        ListFieldMap<TEntity> fields,
        Func<KeysetRequest, CancellationToken, Task<KeysetPage<T>>> query)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(query);
        var catalog = context.RequestServices.GetRequiredService<ErrorCatalog>();
        var validationFailed = catalog.Prefix + ErrorCatalog.ValidationFailed;

        var (list, errors) = FilterSortGrammar.Parse(context.Request.QueryString.Value, fields);
        if (list is null)
        {
            return new NibrasProblemResult(validationFailed, "The list parameters are not valid.", errors: errors);
        }

        var cursors = context.RequestServices.GetRequiredService<ListCursors>();
        var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? context.Request.Path.Value ?? "";
        IReadOnlyList<object?>? after = null;
        var direction = KeysetDirection.Next;
        if (list.Cursor is { } cursor)
        {
            if (cursors.Decode(cursor, fields, list.Sort, list.NormalizedSort, list.FilterHash, route) is not { } decoded)
            {
                return new NibrasProblemResult(
                    validationFailed, "The cursor does not belong to this list, sort and filter.", errors: [new FieldError("cursor", "invalid")]);
            }

            (after, direction) = decoded;
        }

        var page = await query(new KeysetRequest(list.PageSize, list.Sort, list.Filters, after, direction), context.RequestAborted)
            .ConfigureAwait(false);

        string? Cursor(IReadOnlyList<object?>? key, KeysetDirection d) =>
            key is null ? null : cursors.Encode(key, list.NormalizedSort, list.FilterHash, d, route);

        var forward = direction == KeysetDirection.Next;
        var hasMore = forward ? page.HasMoreInDirection : page.Items.Count > 0;
        var hasPrevious = forward ? after is not null && page.Items.Count > 0 : page.HasMoreInDirection;
        return TypedResults.Ok(new PaginationEnvelope<T>(
            page.Items,
            list.PageSize,
            hasMore ? Cursor(page.LastKey, KeysetDirection.Next) : null,
            hasPrevious ? Cursor(page.FirstKey, KeysetDirection.Previous) : null,
            hasMore,
            list.NormalizedSort,
            list.FilterHash));
    }
}
