namespace Nibras.BuildingBlocks.Application.Lists;

/// <summary>
/// The operators of the filter grammar (document 22 §3.1). As a set, what a field accepts; as a single value,
/// the operator of one predicate.
/// </summary>
[Flags]
public enum FilterOperators
{
    None = 0,
    Eq = 1,
    Ne = 1 << 1,
    Gt = 1 << 2,
    Gte = 1 << 3,
    Lt = 1 << 4,
    Lte = 1 << 5,
    In = 1 << 6,
    Nin = 1 << 7,
    Contains = 1 << 8,
    StartsWith = 1 << 9,
    IsNull = 1 << 10,
    Between = 1 << 11,

    /// <summary>Equal, not equal, in a set, not in a set.</summary>
    Equality = Eq | Ne | In | Nin,

    /// <summary>Equality and ordering: dates, instants, numbers.</summary>
    Range = Equality | Gt | Gte | Lt | Lte | Between,

    /// <summary>Equality and the case-insensitive text matches.</summary>
    Text = Equality | Contains | StartsWith,
}

/// <summary>One parsed predicate. <see cref="Values"/> are already of the field's declared type.</summary>
public sealed record FilterPredicate(string Field, FilterOperators Operator, IReadOnlyList<object?> Values);

/// <summary>One sort key; the last one of a request is always the id.</summary>
public sealed record SortField(string Field, bool Descending)
{
    public override string ToString() => (Descending ? "-" : "+") + Field;
}

public enum KeysetDirection
{
    Next,
    Previous,
}

/// <summary>
/// A keyset list query after parsing: the page size already clamped, the sort ending in the id, the filters
/// typed, and, after the first page, the sort-key values of the row the page continues from.
/// </summary>
public sealed record KeysetRequest(
    int PageSize,
    IReadOnlyList<SortField> Sort,
    IReadOnlyList<FilterPredicate> Filters,
    IReadOnlyList<object?>? After = null,
    KeysetDirection Direction = KeysetDirection.Next);

/// <summary>
/// One page in display order, whether more rows exist beyond it in the direction it was read, and the sort-key
/// values of its first and last rows from which the cursors are made.
/// </summary>
public sealed record KeysetPage<T>(
    IReadOnlyList<T> Items,
    bool HasMoreInDirection,
    IReadOnlyList<object?>? FirstKey,
    IReadOnlyList<object?>? LastKey);
