using System.Linq.Expressions;

namespace Nibras.BuildingBlocks.Application.Lists;

/// <summary>A field a list endpoint declares: its wire name, where it comes from, what may filter and sort it.</summary>
public sealed class ListField
{
    internal ListField(string name, LambdaExpression selector, FilterOperators operators, bool sortable)
    {
        Name = name;
        Selector = selector;
        ValueType = selector.ReturnType;
        var underlying = Nullable.GetUnderlyingType(ValueType);
        CoreType = underlying ?? ValueType;
        IsNullable = underlying is not null || !ValueType.IsValueType;
        Operators = operators;
        Sortable = sortable;
    }

    /// <summary>The camelCase wire name, at most two levels: <c>lastName</c>, <c>balance.amount</c>.</summary>
    public string Name { get; }

    /// <summary>The member of the entity, as an expression the query provider translates.</summary>
    public LambdaExpression Selector { get; }

    public Type ValueType { get; }

    /// <summary>The type without <see cref="Nullable{T}"/>.</summary>
    public Type CoreType { get; }

    public bool IsNullable { get; }

    public FilterOperators Operators { get; }

    public bool Sortable { get; }
}

/// <summary>Creates field maps; the page-size limits every list shares.</summary>
public static class ListFieldMap
{
    /// <summary>The largest page any list serves (document 22 §2.1).</summary>
    public const int AbsoluteMaxPageSize = 200;

    public const int DefaultPageSize = 50;

    /// <summary>Starts a map with the UUID v7 id, the documented default sort and the endpoint's maximum page size.</summary>
    public static ListFieldMap<TEntity> For<TEntity>(Expression<Func<TEntity, Guid>> id, string defaultSort, int maxPageSize = AbsoluteMaxPageSize)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultSort);
        return new ListFieldMap<TEntity>(id, defaultSort, maxPageSize);
    }
}

/// <summary>
/// What a list endpoint lets a caller filter and sort, declared once beside the query (document 22 §3).
/// Anything not declared is refused with 400; a field classified Sensitive in Appendix J is simply never declared.
/// </summary>
public sealed class ListFieldMap<TEntity>
{
    private static readonly HashSet<Type> Supported =
    [
        typeof(string), typeof(Guid), typeof(bool), typeof(int), typeof(long), typeof(decimal),
        typeof(DateOnly), typeof(TimeOnly), typeof(DateTimeOffset), typeof(DateTime),
    ];

    private readonly Dictionary<string, ListField> _fields = new(StringComparer.Ordinal);

    internal ListFieldMap(Expression<Func<TEntity, Guid>> id, string defaultSort, int maxPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPageSize, 1);
        Id = new ListField("id", id, FilterOperators.Eq | FilterOperators.In, sortable: true);
        _fields.Add(Id.Name, Id);
        DefaultSort = defaultSort;
        MaxPageSize = Math.Min(maxPageSize, ListFieldMap.AbsoluteMaxPageSize);
    }

    public ListField Id { get; }

    /// <summary>The sort used when the caller sends none, for example <c>+lastName</c>; <c>+id</c> is appended.</summary>
    public string DefaultSort { get; }

    /// <summary>The declared maximum, never above <see cref="ListFieldMap.AbsoluteMaxPageSize"/>.</summary>
    public int MaxPageSize { get; }

    public IReadOnlyDictionary<string, ListField> Fields => _fields;

    /// <summary>Declares one field. Operators the type cannot honour are refused here, at startup, not per request.</summary>
    public ListFieldMap<TEntity> Field<TValue>(
        string name,
        Expression<Func<TEntity, TValue>> selector,
        FilterOperators filter = FilterOperators.None,
        bool sortable = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(selector);
        if (!IsFieldName(name))
        {
            throw new ArgumentException($"'{name}' is not a camelCase field name of at most two levels.", nameof(name));
        }

        var field = new ListField(name, selector, filter, sortable);
        var core = field.CoreType;
        if (!Supported.Contains(core) && !core.IsEnum)
        {
            throw new ArgumentException($"Field '{name}' has type {core.Name}, which the list grammar cannot parse.", nameof(selector));
        }

        const FilterOperators ordering = FilterOperators.Gt | FilterOperators.Gte | FilterOperators.Lt | FilterOperators.Lte | FilterOperators.Between;
        if ((filter & ordering) != 0 && (core.IsEnum || core == typeof(bool) || core == typeof(Guid)))
        {
            throw new ArgumentException($"Field '{name}' of type {core.Name} has no meaningful order to filter on.", nameof(filter));
        }

        if ((filter & (FilterOperators.Contains | FilterOperators.StartsWith)) != 0 && core != typeof(string))
        {
            throw new ArgumentException($"Field '{name}' is not text, so contains and startsWith do not apply.", nameof(filter));
        }

        if ((filter & FilterOperators.IsNull) != 0 && !field.IsNullable)
        {
            throw new ArgumentException($"Field '{name}' can never be null.", nameof(filter));
        }

        if (!_fields.TryAdd(name, field))
        {
            throw new ArgumentException($"Field '{name}' is declared twice.", nameof(name));
        }

        return this;
    }

    private static bool IsFieldName(string name)
    {
        var parts = name.Split('.');
        return parts.Length <= 2 && parts.All(p =>
            p.Length > 0 && p[0] is >= 'a' and <= 'z' && p.All(c => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9')));
    }
}
