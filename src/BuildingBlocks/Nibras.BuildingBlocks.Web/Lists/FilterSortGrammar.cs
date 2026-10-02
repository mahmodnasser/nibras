using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Nibras.BuildingBlocks.Application.Lists;

namespace Nibras.BuildingBlocks.Web.Lists;

/// <summary>A list request after the grammar: what the query needs, plus the normalized sort and filter hash the envelope echoes.</summary>
public sealed record ListQuery(
    int PageSize,
    IReadOnlyList<SortField> Sort,
    IReadOnlyList<FilterPredicate> Filters,
    string? Cursor,
    string NormalizedSort,
    string FilterHash);

/// <summary>
/// The one filter and sort grammar of document 22 §3, parsed once against the endpoint's declared field map.
/// It works on the raw query string, so a percent-encoded <c>,</c> or <c>..</c> inside a value is split correctly.
/// </summary>
public static partial class FilterSortGrammar
{
    public const int MaxPredicates = 10;
    public const int MaxListMembers = 100;
    public const int MaxValueLength = 200;
    public const int MaxCursorLength = 512;
    public const int MaxSortFields = 3;

    private static readonly Dictionary<string, FilterOperators> Operators = new(StringComparer.Ordinal)
    {
        ["eq"] = FilterOperators.Eq,
        ["ne"] = FilterOperators.Ne,
        ["gt"] = FilterOperators.Gt,
        ["gte"] = FilterOperators.Gte,
        ["lt"] = FilterOperators.Lt,
        ["lte"] = FilterOperators.Lte,
        ["in"] = FilterOperators.In,
        ["nin"] = FilterOperators.Nin,
        ["contains"] = FilterOperators.Contains,
        ["startsWith"] = FilterOperators.StartsWith,
        ["isNull"] = FilterOperators.IsNull,
        ["between"] = FilterOperators.Between,
    };

    /// <summary>Parses <paramref name="rawQuery"/> (with or without the leading <c>?</c>); on failure, one field error per problem.</summary>
    public static (ListQuery? Query, IReadOnlyList<FieldError> Errors) Parse<TEntity>(string? rawQuery, ListFieldMap<TEntity> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        var errors = new List<FieldError>();
        string? pageSizeText = null, sortText = null, cursor = null;
        var raw = new List<(string Key, string Field, string Operator, string RawValue)>();

        foreach (var pair in (rawQuery ?? "").TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var equals = pair.IndexOf('=', StringComparison.Ordinal);
            var key = WebUtility.UrlDecode(equals < 0 ? pair : pair[..equals]);
            var rawValue = equals < 0 ? "" : pair[(equals + 1)..];
            switch (key)
            {
                case "pageSize":
                    pageSizeText = Once(errors, key, pageSizeText, WebUtility.UrlDecode(rawValue));
                    continue;
                case "sort":
                    sortText = Once(errors, key, sortText, WebUtility.UrlDecode(rawValue));
                    continue;
                case "cursor":
                    cursor = Once(errors, key, cursor, WebUtility.UrlDecode(rawValue));
                    continue;
                case "q":
                    errors.Add(new FieldError("q", "notSearchable")); // an endpoint's declared search arrives with Localization (BR-L10N-001)
                    continue;
            }

            var match = FilterKey().Match(key);
            if (match.Success)
            {
                var op = match.Groups[2].Success ? match.Groups[2].Value : "eq";
                raw.Add((key, match.Groups[1].Value, op, rawValue));
            }
        }

        var pageSize = PageSize(pageSizeText, fields.MaxPageSize, errors);
        var sort = Sort(sortText ?? fields.DefaultSort, fields, sortText is null, errors);
        if (cursor is { Length: > MaxCursorLength })
        {
            errors.Add(new FieldError("cursor", "maxLength", Limit("maxLength", MaxCursorLength)));
        }

        var filters = Filters(raw, fields, errors);
        if (errors.Count > 0)
        {
            return (null, errors);
        }

        return (new ListQuery(pageSize, sort, filters, cursor, string.Join(',', sort), HashOf(filters)), errors);
    }

    private static string? Once(List<FieldError> errors, string key, string? existing, string value)
    {
        if (existing is not null)
        {
            errors.Add(new FieldError(key, "repeated"));
        }

        return value;
    }

    /// <summary>Default 50; above the declared maximum is clamped, not refused (REQ-API-007).</summary>
    private static int PageSize(string? text, int max, List<FieldError> errors)
    {
        if (text is null)
        {
            return Math.Min(ListFieldMap.DefaultPageSize, max);
        }

        if (!int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var size) || size < 1)
        {
            if (text.Length > 0 && text.All(char.IsAsciiDigit) && text.TrimStart('0').Length > 0)
            {
                return max; // a number too large for an int is still a request above the maximum
            }

            errors.Add(new FieldError("pageSize", "outOfRange", Limit("min", 1)));
            return 0;
        }

        return Math.Min(size, max);
    }

    private static List<SortField> Sort<TEntity>(string text, ListFieldMap<TEntity> fields, bool isDefault, List<FieldError> errors)
    {
        var sort = new List<SortField>();
        var hasId = false;
        foreach (var part in text.Split(','))
        {
            // A literal '+' in a query string decodes to a space, so a leading space is the ascending sign too.
            var descending = part.StartsWith('-');
            var name = part.TrimStart('+', '-', ' ');
            if (hasId)
            {
                errors.Add(new FieldError("sort", "idMustBeLast"));
                break;
            }

            if (!fields.Fields.TryGetValue(name, out var field) || !field.Sortable)
            {
                Defect(isDefault, "default sort names an undeclared field");
                errors.Add(new FieldError("sort", "notSortable", new System.Text.Json.Nodes.JsonObject { ["field"] = name }));
                continue;
            }

            if (sort.Any(s => s.Field == name))
            {
                errors.Add(new FieldError("sort", "repeated", new System.Text.Json.Nodes.JsonObject { ["field"] = name }));
                continue;
            }

            hasId = field == fields.Id;
            sort.Add(new SortField(name, descending));
        }

        if (sort.Count(s => s.Field != fields.Id.Name) > MaxSortFields)
        {
            Defect(isDefault, "default sort has more than three fields");
            errors.Add(new FieldError("sort", "tooManyFields", Limit("max", MaxSortFields)));
        }

        if (!hasId)
        {
            sort.Add(new SortField(fields.Id.Name, Descending: false));
        }

        return sort;
    }

    private static List<FilterPredicate> Filters<TEntity>(
        List<(string Key, string Field, string Operator, string RawValue)> raw, ListFieldMap<TEntity> fields, List<FieldError> errors)
    {
        var filters = new List<FilterPredicate>();
        if (raw.Count > MaxPredicates)
        {
            errors.Add(new FieldError("filter", "tooManyPredicates", Limit("max", MaxPredicates)));
            return filters;
        }

        var seen = new HashSet<(string, string)>();
        foreach (var (key, name, opText, rawValue) in raw)
        {
            if (!seen.Add((name, opText)))
            {
                errors.Add(new FieldError(key, "repeated"));
                continue;
            }

            if (!fields.Fields.TryGetValue(name, out var field) || field.Operators == FilterOperators.None)
            {
                errors.Add(new FieldError(key, "notFilterable"));
                continue;
            }

            if (!Operators.TryGetValue(opText, out var op) || (field.Operators & op) == 0)
            {
                errors.Add(new FieldError(key, "operatorNotAllowed"));
                continue;
            }

            var parts = op switch
            {
                FilterOperators.In or FilterOperators.Nin => rawValue.Split(','),
                FilterOperators.Between => rawValue.Split("..", 2),
                _ => [rawValue],
            };
            if (op == FilterOperators.Between && parts.Length != 2)
            {
                errors.Add(new FieldError(key, "invalidRange"));
                continue;
            }

            if (parts.Length > MaxListMembers)
            {
                errors.Add(new FieldError(key, "tooManyValues", Limit("max", MaxListMembers)));
                continue;
            }

            var values = new List<object?>(parts.Length);
            var valueType = op == FilterOperators.IsNull ? typeof(bool) : field.CoreType;
            foreach (var part in parts)
            {
                var text = WebUtility.UrlDecode(part);
                if (text.Length > MaxValueLength)
                {
                    errors.Add(new FieldError(key, "maxLength", Limit("maxLength", MaxValueLength)));
                    values = null;
                    break;
                }

                if (!ListValues.TryParse(text, valueType, out var value) || (value is string s && s.Length == 0 && op != FilterOperators.Eq))
                {
                    errors.Add(new FieldError(key, "invalidValue"));
                    values = null;
                    break;
                }

                values.Add(value);
            }

            if (values is not null)
            {
                filters.Add(new FilterPredicate(name, op, values));
            }
        }

        return filters;
    }

    /// <summary>A short hash of the normalized filter; the cursor carries it, so changing the filter invalidates the cursor.</summary>
    internal static string HashOf(IReadOnlyList<FilterPredicate> filters)
    {
        var canonical = string.Join('&', filters
            .Select(f => f.Field + ":" + f.Operator + "=" + string.Join(',', f.Values.Select(ListValues.Format)))
            .Order(StringComparer.Ordinal));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)).AsSpan(0, 2));
    }

    private static System.Text.Json.Nodes.JsonObject Limit(string name, int value) => new() { [name] = value };

    private static void Defect(bool isDefault, string message)
    {
        if (isDefault)
        {
            throw new InvalidOperationException("The list's " + message + "; fix its field map.");
        }
    }

    [GeneratedRegex(@"^filter\[([^\[\]]+)\](?:\[([A-Za-z]+)\])?$", RegexOptions.CultureInvariant)]
    private static partial Regex FilterKey();
}
