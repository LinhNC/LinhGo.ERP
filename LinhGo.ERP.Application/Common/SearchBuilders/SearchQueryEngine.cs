using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace LinhGo.ERP.Application.Common.SearchBuilders;

// QueryEngine.cs
public sealed class SearchQueryEngine<T>
    where T : class
{
    private readonly IReadOnlyDictionary<string, Expression<Func<T, object>>> _filterMap;
    private readonly IReadOnlyDictionary<string, LambdaExpression> _sortMap;
    private readonly Func<IQueryable<T>, string?, SearchQueryParams, CancellationToken, Task<PagedResult<T>>> _executorCached = null!;

    public SearchQueryEngine(
        IReadOnlyDictionary<string, Expression<Func<T, object>>> filterMap,
        IReadOnlyDictionary<string, LambdaExpression> sortMap)
    {
        _filterMap = filterMap;
        _sortMap = sortMap;

        // prebuild executor for performance? Optional advanced step
    }

    /// <summary>
    /// Execute query against source IQueryable with projection selector.
    /// </summary>
    public async Task<PagedResult<TResult>> ExecuteAsync<TResult>(
        IQueryable<T> source,
        SearchQueryParams qp,
        Expression<Func<T, TResult>> selector,
        Func<string, bool>? includeIsAllowed = null,
        Func<IQueryable<T>, string?, IQueryable<T>>? includeApplier = null,
        CancellationToken ct = default)
    {
        // includes (string-based safe include via allowed checker)
        if (!string.IsNullOrWhiteSpace(qp.Include) && includeIsAllowed != null && includeApplier != null)
            source = includeApplier(source, qp.Include);

        // filters
        if (qp.Filter != null && qp.Filter.Count > 0)
            source = ApplyFilters(source, qp.Filter);

        // search (q) - user can customize
        if (!string.IsNullOrWhiteSpace(qp.Q))
        {
            // Default: if target has a Title or Name mapping, you can wire a custom search; for generic engine, consumer passes a pre-filtered source or use selector
            // For sample: if T has Title property mapped in filterMap, use contains on that field as fallback.
            if (_filterMap.TryGetValue("title", out var titleExpr))
            {
                var param = titleExpr.Parameters[0];
                var body = titleExpr.Body is UnaryExpression u ? u.Operand : titleExpr.Body;
                var contains = BuildStringContains(body, qp.Q);
                var lambda = Expression.Lambda<Func<T, bool>>(contains, param);
                source = source.Where(lambda);
            }
        }

        // total (before paging)
        var total = await source.CountAsync(ct);

        // sorting
        source = ApplySort(source, qp.Sort);

        if (string.IsNullOrWhiteSpace(qp.Sort))
        {
            // default ordering if none (best-effort): choose createdAt if exists
            if (_filterMap.ContainsKey("createdAt"))
            {
                var createdExpr = (Expression<Func<T, object>>)_filterMap["createdAt"];
                source = source.OrderByDescending(createdExpr);
            }
        }

        // paging
        var page = Math.Max(1, qp.Page);
        var pageSize = Math.Clamp(qp.PageSize, 1, 500);
        var skip = (page - 1) * pageSize;

        var items = await source
            .Skip(skip)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(ct);

        return new PagedResult<TResult>()
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private IQueryable<T> ApplyFilters(IQueryable<T> source, IReadOnlyDictionary<string, Dictionary<string, string>> filters)
    {
        // Build a combined predicate expression and apply once
        var param = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var kv in filters)
        {
            var field = kv.Key;
            if (!_filterMap.TryGetValue(field, out var memberExpr)) continue;

            var body = ReplaceParameter(memberExpr.Body, memberExpr.Parameters[0], param);
            body = UnwrapToActualType(body);

            Expression? fieldCombined = null;
            foreach (var opKvp in kv.Value)
            {
                var op = opKvp.Key.ToLowerInvariant();
                var raw = opKvp.Value;
                Expression? cur = BuildExpressionForOperator(body, op, raw);
                if (cur == null) continue;
                fieldCombined = fieldCombined == null ? cur : Expression.AndAlso(fieldCombined, cur);
            }

            if (fieldCombined != null) combined = combined == null ? fieldCombined : Expression.AndAlso(combined, fieldCombined);
        }

        if (combined == null) return source;
        var lambda = Expression.Lambda<Func<T, bool>>(combined, param);
        return source.Where(lambda);
    }

    private IQueryable<T> ApplySort(IQueryable<T> source, string? sortQuery)
    {
        if (string.IsNullOrWhiteSpace(sortQuery)) return source;

        var parts = sortQuery.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
        bool first = true;
        IOrderedQueryable<T>? ordered = null;

        foreach (var raw in parts)
        {
            if (raw.Length == 0) continue;
            var desc = raw.StartsWith("-");
            var key = desc ? raw[1..] : raw;

            if (!_sortMap.TryGetValue(key, out var lambdaExpr)) continue;

            // cast to concrete type
            var typed = (Expression<Func<T, object>>)lambdaExpr;
            if (first)
            {
                ordered = desc ? source.OrderByDescending(typed) : source.OrderBy(typed);
                first = false;
            }
            else
            {
                ordered = desc ? ordered!.ThenByDescending(typed) : ordered!.ThenBy(typed);
            }
        }

        return ordered ?? source;
    }

    #region Expression helpers
    private static Expression ReplaceParameter(Expression expr, ParameterExpression from, ParameterExpression to)
        => new ParamReplaceVisitor(from, to).Visit(expr)!;

    private sealed class ParamReplaceVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }

    private static Expression UnwrapToActualType(Expression expr)
    {
        if (expr is UnaryExpression u && (u.NodeType == ExpressionType.Convert || u.NodeType == ExpressionType.ConvertChecked))
            return u.Operand;
        return expr;
    }

    private static Expression? BuildExpressionForOperator(Expression member, string op, string raw)
    {
        // member: expression of actual type (not boxed)
        // returns Expression (bool) or null
        if (string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
        {
            return op switch
            {
                "eq" => Expression.Equal(member, Expression.Constant(null, member.Type)),
                "neq" or "ne" or "notnull" => Expression.NotEqual(member, Expression.Constant(null, member.Type)),
                _ => null
            };
        }

        if (op == "in")
        {
            var items = raw.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            Expression? acc = null;
            foreach (var it in items)
            {
                var parsed = ParseRawToType(it, member.Type);
                if (parsed == null) continue;
                var eq = Expression.Equal(member, Expression.Constant(parsed, member.Type));
                acc = acc == null ? eq : Expression.OrElse(acc, eq);
            }
            return acc;
        }

        if (op == "contains" || op == "startswith" || op == "endswith")
        {
            // ensure string; if not string, call ToString()
            var strExpr = member.Type == typeof(string) ? member : Expression.Call(member, "ToString", Type.EmptyTypes);
            var method = typeof(string).GetMethod(op == "contains" ? nameof(string.Contains) :
                op == "startswith" ? nameof(string.StartsWith) : nameof(string.EndsWith), new[] { typeof(string) })!;
            return Expression.Call(strExpr, method, Expression.Constant(raw));
        }

        // numeric/date/enum comparisons
        var parsedVal = ParseRawToType(raw, member.Type);
        if (parsedVal == null) return null;
        var right = Expression.Constant(parsedVal, member.Type);

        return op switch
        {
            "eq" => Expression.Equal(member, right),
            "neq" or "ne" => Expression.NotEqual(member, right),
            "gt" => Expression.GreaterThan(member, right),
            "gte" => Expression.GreaterThanOrEqual(member, right),
            "lt" => Expression.LessThan(member, right),
            "lte" => Expression.LessThanOrEqual(member, right),
            _ => null
        };
    }

    private static object? ParseRawToType(string? raw, Type target)
    {
        if (raw == null) return null;
        if (raw.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;

        var nt = Nullable.GetUnderlyingType(target) ?? target;
        try
        {
            if (nt == typeof(string)) return raw;
            if (nt == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (nt == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
            if (nt == typeof(decimal)) return decimal.Parse(raw, CultureInfo.InvariantCulture);
            if (nt == typeof(double)) return double.Parse(raw, CultureInfo.InvariantCulture);
            if (nt == typeof(bool)) return bool.Parse(raw);
            if (nt == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            if (nt.IsEnum) return Enum.Parse(nt, raw, true);
            return Convert.ChangeType(raw, nt, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static Expression BuildStringContains(Expression member, string raw)
    {
        Expression strExpr = member.Type == typeof(string) ? member : Expression.Call(member, "ToString", Type.EmptyTypes);
        var method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        return Expression.Call(strExpr, method, Expression.Constant(raw));
    }
    #endregion
}
