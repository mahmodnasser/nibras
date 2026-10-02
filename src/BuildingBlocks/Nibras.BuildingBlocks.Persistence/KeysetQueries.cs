using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nibras.BuildingBlocks.Application.Lists;

namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// Keyset pagination (master brief Section 19, document 22 §2.1): the declared filters, the sort ending in the
/// id, the continuation predicate on the last row's key values, and <c>pageSize + 1</c> rows in one command,
/// untracked and projected to the caller's DTO. A previous page is read in the reversed order and turned back.
/// </summary>
public static class KeysetQueries
{
    private static readonly MethodInfo StringCompare = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo ILike = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
        nameof(NpgsqlDbFunctionsExtensions.ILike), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

    private static readonly MethodInfo Contains = typeof(Enumerable).GetMethods()
        .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

    public static async Task<KeysetPage<TDto>> ToKeysetPageAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        KeysetRequest request,
        ListFieldMap<TEntity> fields,
        Expression<Func<TEntity, TDto>> projection,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(projection);
        if (request.Sort.Count == 0 || request.Sort[^1].Field != fields.Id.Name)
        {
            throw new ArgumentException("A keyset sort ends in the id, so the page boundary is deterministic.", nameof(request));
        }

        var entity = Expression.Parameter(typeof(TEntity), "e");
        var source = query.AsNoTracking();
        foreach (var predicate in request.Filters)
        {
            source = source.Where(Lambda<TEntity>(Filter(Field(fields, predicate.Field), predicate, entity), entity));
        }

        var reversed = request.Direction == KeysetDirection.Previous;
        var order = request.Sort
            .Select(s => (Field: Field(fields, s.Field), Descending: s.Descending ^ reversed))
            .ToArray();
        if (request.After is { } after)
        {
            if (after.Count != order.Length)
            {
                throw new ArgumentException("The cursor carries one value per sort field.", nameof(request));
            }

            source = source.Where(Lambda<TEntity>(After(order, after, entity), entity));
        }

        var ordered = ApplyOrder(source, order, entity);
        var rows = await ordered
            .Select(Row(projection, order.Select(o => o.Field), entity))
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hasMore = rows.Count > request.PageSize;
        if (hasMore)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        if (reversed)
        {
            rows.Reverse();
        }

        return new KeysetPage<TDto>(
            rows.Select(r => r.Item).ToArray(),
            hasMore,
            rows.Count == 0 ? null : rows[0].Keys,
            rows.Count == 0 ? null : rows[^1].Keys);
    }

    /// <summary>The projected row and its sort-key values, built in the final projection of the same command.</summary>
    internal sealed class KeysetRow<TDto>
    {
        public TDto Item { get; set; } = default!;

        public object?[] Keys { get; set; } = [];
    }

    private static ListField Field<TEntity>(ListFieldMap<TEntity> fields, string name) =>
        fields.Fields.TryGetValue(name, out var field)
            ? field
            : throw new ArgumentException($"Field '{name}' is not declared for this list.", nameof(name));

    private static Expression<Func<TEntity, bool>> Lambda<TEntity>(Expression body, ParameterExpression entity) =>
        Expression.Lambda<Func<TEntity, bool>>(body, entity);

    private static Expression Body(ListField field, ParameterExpression entity) =>
        new ParameterReplacer(field.Selector.Parameters[0], entity).Visit(field.Selector.Body);

    private static Expression Filter(ListField field, FilterPredicate predicate, ParameterExpression entity)
    {
        var member = Body(field, entity);
        var values = predicate.Values;
        return predicate.Operator switch
        {
            FilterOperators.Eq => Expression.Equal(member, Parameter(values[0], field.ValueType)),
            FilterOperators.Ne => Expression.NotEqual(member, Parameter(values[0], field.ValueType)),
            FilterOperators.Gt => Compare(member, Parameter(values[0], field.ValueType), ExpressionType.GreaterThan),
            FilterOperators.Gte => Compare(member, Parameter(values[0], field.ValueType), ExpressionType.GreaterThanOrEqual),
            FilterOperators.Lt => Compare(member, Parameter(values[0], field.ValueType), ExpressionType.LessThan),
            FilterOperators.Lte => Compare(member, Parameter(values[0], field.ValueType), ExpressionType.LessThanOrEqual),
            FilterOperators.Between => Expression.AndAlso(
                Compare(member, Parameter(values[0], field.ValueType), ExpressionType.GreaterThanOrEqual),
                Compare(member, Parameter(values[1], field.ValueType), ExpressionType.LessThanOrEqual)),
            FilterOperators.In => InSet(member, field.ValueType, values),
            FilterOperators.Nin => Expression.Not(InSet(member, field.ValueType, values)),
            FilterOperators.Contains => Like(member, "%" + EscapeLike((string)values[0]!) + "%"),
            FilterOperators.StartsWith => Like(member, EscapeLike((string)values[0]!) + "%"),
            FilterOperators.IsNull => (bool)values[0]!
                ? Expression.Equal(member, Expression.Constant(null, field.ValueType))
                : Expression.NotEqual(member, Expression.Constant(null, field.ValueType)),
            _ => throw new ArgumentException($"Operator {predicate.Operator} is not one operator.", nameof(predicate)),
        };
    }

    /// <summary>
    /// The rows strictly after the key in the effective order. PostgreSQL sorts nulls last ascending and first
    /// descending, so after a non-null value ascending the nulls still follow, and after a null descending only
    /// non-null values follow. The id is never null and unique, so the expansion ends there.
    /// </summary>
    private static Expression After((ListField Field, bool Descending)[] order, IReadOnlyList<object?> key, ParameterExpression entity)
    {
        Expression? result = null;
        Expression? equalSoFar = null;
        for (var i = 0; i < order.Length; i++)
        {
            var (field, descending) = order[i];
            var member = Body(field, entity);
            var value = key[i];
            Expression strictlyAfter = value is null
                ? descending ? Expression.NotEqual(member, Expression.Constant(null, field.ValueType)) : Expression.Constant(false)
                : descending
                    ? Compare(member, Parameter(value, field.ValueType), ExpressionType.LessThan)
                    : field.IsNullable
                        ? Expression.OrElse(
                            Compare(member, Parameter(value, field.ValueType), ExpressionType.GreaterThan),
                            Expression.Equal(member, Expression.Constant(null, field.ValueType)))
                        : Compare(member, Parameter(value, field.ValueType), ExpressionType.GreaterThan);

            var branch = equalSoFar is null ? strictlyAfter : Expression.AndAlso(equalSoFar, strictlyAfter);
            result = result is null ? branch : Expression.OrElse(result, branch);

            var equal = value is null
                ? Expression.Equal(member, Expression.Constant(null, field.ValueType))
                : Expression.Equal(member, Parameter(value, field.ValueType));
            equalSoFar = equalSoFar is null ? equal : Expression.AndAlso(equalSoFar, equal);
        }

        return result!;
    }

    private static IQueryable<TEntity> ApplyOrder<TEntity>(IQueryable<TEntity> source, (ListField Field, bool Descending)[] order, ParameterExpression entity)
    {
        var expression = source.Expression;
        for (var i = 0; i < order.Length; i++)
        {
            var (field, descending) = order[i];
            var key = Expression.Lambda(Body(field, entity), entity);
            var method = (i == 0, descending) switch
            {
                (true, false) => nameof(Queryable.OrderBy),
                (true, true) => nameof(Queryable.OrderByDescending),
                (false, false) => nameof(Queryable.ThenBy),
                (false, true) => nameof(Queryable.ThenByDescending),
            };
            expression = Expression.Call(typeof(Queryable), method, [typeof(TEntity), field.ValueType], expression, Expression.Quote(key));
        }

        return source.Provider.CreateQuery<TEntity>(expression);
    }

    private static Expression<Func<TEntity, KeysetRow<TDto>>> Row<TEntity, TDto>(
        Expression<Func<TEntity, TDto>> projection, IEnumerable<ListField> keys, ParameterExpression entity)
    {
        var item = new ParameterReplacer(projection.Parameters[0], entity).Visit(projection.Body);
        var values = keys.Select(k => (Expression)Expression.Convert(Body(k, entity), typeof(object)));
        var row = Expression.MemberInit(
            Expression.New(typeof(KeysetRow<TDto>)),
            Expression.Bind(typeof(KeysetRow<TDto>).GetProperty(nameof(KeysetRow<TDto>.Item))!, item),
            Expression.Bind(typeof(KeysetRow<TDto>).GetProperty(nameof(KeysetRow<TDto>.Keys))!, Expression.NewArrayInit(typeof(object), values)));
        return Expression.Lambda<Func<TEntity, KeysetRow<TDto>>>(row, entity);
    }

    /// <summary>
    /// An ordering comparison the provider translates for every declared type: <c>string.Compare</c> for text,
    /// <c>CompareTo</c> for identifiers, the underlying number for enums and 0 or 1 for booleans.
    /// </summary>
    private static BinaryExpression Compare(Expression left, Expression right, ExpressionType kind)
    {
        var type = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
        if (type == typeof(string))
        {
            return Expression.MakeBinary(kind, Expression.Call(StringCompare, left, right), Expression.Constant(0));
        }

        if (type == typeof(Guid))
        {
            return Expression.MakeBinary(kind, Expression.Call(left, nameof(Guid.CompareTo), null, right), Expression.Constant(0));
        }

        if (type.IsEnum)
        {
            var number = left.Type == type ? Enum.GetUnderlyingType(type) : typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(type));
            return Expression.MakeBinary(kind, Expression.Convert(left, number), Expression.Convert(right, number));
        }

        if (type == typeof(bool))
        {
            return Expression.MakeBinary(kind, AsNumber(left), AsNumber(right));
        }

        return Expression.MakeBinary(kind, left, right);
    }

    private static ConditionalExpression AsNumber(Expression boolean) =>
        Expression.Condition(Expression.Equal(boolean, Expression.Constant(true, boolean.Type)), Expression.Constant(1), Expression.Constant(0));

    private static MethodCallExpression InSet(Expression member, Type type, IReadOnlyList<object?> values)
    {
        var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        foreach (var value in values)
        {
            list.Add(value);
        }

        return Expression.Call(Contains.MakeGenericMethod(type), Parameter(list, list.GetType()), member);
    }

    private static MethodCallExpression Like(Expression member, string pattern) =>
        Expression.Call(
            ILike,
            Expression.Property(null, typeof(EF), nameof(EF.Functions)),
            member,
            Parameter(pattern, typeof(string)),
            Expression.Constant("\\"));

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    /// <summary>A value read through a closure, which EF Core sends as a SQL parameter rather than inlining it into the plan.</summary>
    private static MemberExpression Parameter(object? value, Type type)
    {
        var box = Activator.CreateInstance(typeof(Box<>).MakeGenericType(type))!;
        var property = box.GetType().GetProperty(nameof(Box<object>.Value))!;
        property.SetValue(box, value);
        return Expression.Property(Expression.Constant(box), property);
    }

    private sealed class Box<T>
    {
        public T Value { get; set; } = default!;
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}
