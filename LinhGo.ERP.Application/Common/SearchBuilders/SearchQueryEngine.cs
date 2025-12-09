using System.Globalization;
using System.Linq.Expressions;
using LinhGo.ERP.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LinhGo.ERP.Application.Common.SearchBuilders;

/// <summary>
/// Generic search query engine that applies filters, sorting, and pagination to IQueryable sources
/// Optimized for performance with expression tree caching and efficient query building
/// </summary>
/// <typeparam name="T">Entity type to query</typeparam>
public sealed class SearchQueryEngine<T>
    where T : class
{
    private const int MinPageNumber = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;
    private const string DefaultSortField = "createdAt";
    private const string DefaultSearchField = "name";

    private IQueryable<T>? _source;
    private SearchQueryParams? _queryParams;
    private Expression<Func<T, T>>? _selector;
    private IReadOnlyDictionary<string, Expression<Func<T, object>>>? _filterMap;
    private IReadOnlyDictionary<string, LambdaExpression>? _sortMap;
    private Func<string, bool>? _includeIsAllowed;
    private Func<IQueryable<T>, string?, IQueryable<T>>? _includeApplier;
    
    private PagedResult<T>? _result;
    
    public PagedResult<T>? Result() => _result;

    public void SetSource(IQueryable<T> source)
    {
        _source = source;
    }
    public void SetQueryParams(SearchQueryParams queryParams)
    {
        _queryParams = queryParams;
    }

    public void SetSelector(Expression<Func<T, T>> selector)
    {
        _selector = selector;
    }
    public void SetFilterMapping(IReadOnlyDictionary<string, Expression<Func<T, object>>> filterMap)
    {
        _filterMap = filterMap;
    }
    public void SetSortMapping(IReadOnlyDictionary<string, LambdaExpression> sortMap)
    {
        _sortMap = sortMap;
    }
    public void SetIncludeSettings(
        Func<string, bool>? includeIsAllowed,
        Func<IQueryable<T>, string?, IQueryable<T>>? includeApplier)
    {
        _includeIsAllowed = includeIsAllowed;
        _includeApplier = includeApplier;
    }
    

    /// <summary>
    /// Execute search query with filters, sorting, and pagination
    /// </summary>
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(_source);
        ArgumentNullException.ThrowIfNull(_queryParams);
        _selector ??= s => s;

        // Apply includes for eager loading
        if (_includeIsAllowed is not null && _includeApplier is not null)
        {
            _source = ApplyIncludes(_source, _queryParams.Includes, _includeIsAllowed, _includeApplier);
        }

        if (_queryParams?.Filters != null)
        {
            _source = ApplyFilters(_source, _queryParams.Filters);
        }
        
        if (_queryParams?.Q != null)
        {
            _source = ApplyFullTextSearch(_source, _queryParams.Q);
        }

        // Get total count before pagination
        var totalCount = await _source.CountAsync(ct);

        // Apply sorting
        if (_queryParams?.Sorts != null)
        {
            _source = ApplySortWithDefault(_source, _queryParams.Sorts);
        }

        // Apply pagination
        var (page, pageSize) = NormalizePagination(_queryParams.Page, _queryParams.PageSize);
        var items = await _source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(_selector)
            .ToListAsync(ct);

        _result = new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Apply includes for eager loading of related entities
    /// </summary>
    private IQueryable<T> ApplyIncludes(
        IQueryable<T> source,
        string? includePaths,
        Func<string, bool>? includeIsAllowed,
        Func<IQueryable<T>, string?, IQueryable<T>>? includeApplier)
    {
        if (string.IsNullOrWhiteSpace(includePaths) || includeIsAllowed == null || includeApplier == null)
            return source;

        // Validate and apply includes
        if (includeIsAllowed(includePaths))
            return includeApplier(source, includePaths);

        return source;
    }

    /// <summary>
    /// Apply full-text search across searchable fields
    /// </summary>
    private IQueryable<T> ApplyFullTextSearch(IQueryable<T> source, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return source;

        // Try to find a searchable field (title, name, etc.)
        if (!_filterMap.TryGetValue(DefaultSearchField, out var searchExpr))
            return source;

        var parameter = searchExpr.Parameters[0];
        var body = UnwrapToActualType(searchExpr.Body);
        var containsExpression = BuildStringContains(body, searchTerm);
        var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

        return source.Where(lambda);
    }

    /// <summary>
    /// Apply sorting with default fallback
    /// </summary>
    private IQueryable<T> ApplySortWithDefault(IQueryable<T> source, string? sortQuery)
    {
        var sortedSource = ApplySort(source, sortQuery);

        // If no sort was applied and default sort field exists, apply default descending sort
        if (ReferenceEquals(sortedSource, source) && string.IsNullOrWhiteSpace(sortQuery))
        {
            if (_filterMap.TryGetValue(DefaultSortField, out var defaultSortExpr))
            {
                return source.OrderByDescending(defaultSortExpr);
            }
        }

        return sortedSource;
    }

    /// <summary>
    /// Normalize and clamp pagination values
    /// </summary>
    private static (int page, int pageSize) NormalizePagination(int page, int pageSize)
    {
        var normalizedPage = Math.Max(MinPageNumber, page);
        var normalizedPageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);
        return (normalizedPage, normalizedPageSize);
    }

    /// <summary>
    /// Apply dynamic filters to the query source
    /// Builds a combined expression tree for optimal query performance
    /// </summary>
    private IQueryable<T> ApplyFilters(
        IQueryable<T> source,
        IReadOnlyDictionary<string, Dictionary<string, string>>? filters)
    {
        if (filters == null || filters.Count == 0)
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedPredicate = null;

        foreach (var (fieldName, operators) in filters)
        {
            if (!_filterMap.TryGetValue(fieldName, out var memberExpression))
                continue;

            var fieldBody = ReplaceParameter(memberExpression.Body, memberExpression.Parameters[0], parameter);
            fieldBody = UnwrapToActualType(fieldBody);

            var fieldPredicate = BuildFieldPredicate(fieldBody, operators);
            if (fieldPredicate == null)
                continue;

            combinedPredicate = combinedPredicate == null
                ? fieldPredicate
                : Expression.AndAlso(combinedPredicate, fieldPredicate);
        }

        if (combinedPredicate == null)
            return source;

        var filterLambda = Expression.Lambda<Func<T, bool>>(combinedPredicate, parameter);
        return source.Where(filterLambda);
    }

    /// <summary>
    /// Build predicate for a single field with multiple operators
    /// </summary>
    private static Expression? BuildFieldPredicate(
        Expression fieldBody,
        Dictionary<string, string> operators)
    {
        Expression? fieldPredicate = null;

        foreach (var (op, value) in operators)
        {
            var operatorExpression = BuildExpressionForOperator(
                fieldBody,
                op.ToLowerInvariant(),
                value);

            if (operatorExpression == null)
                continue;

            fieldPredicate = fieldPredicate == null
                ? operatorExpression
                : Expression.AndAlso(fieldPredicate, operatorExpression);
        }

        return fieldPredicate;
    }

    /// <summary>
    /// Apply multi-field sorting to the query source
    /// Format: "field1,-field2,field3" where '-' prefix indicates descending
    /// </summary>
    private IQueryable<T> ApplySort(IQueryable<T> source, string? sortQuery)
    {
        if (string.IsNullOrWhiteSpace(sortQuery))
            return source;

        var sortFields = ParseSortFields(sortQuery);
        if (sortFields.Count == 0)
            return source;

        IOrderedQueryable<T>? orderedQuery = null;

        for (int i = 0; i < sortFields.Count; i++)
        {
            var (fieldName, isDescending) = sortFields[i];

            if (!_sortMap.TryGetValue(fieldName, out var sortExpression))
                continue;

            var typedExpression = (Expression<Func<T, object>>)sortExpression;

            if (orderedQuery == null)
            {
                // First sort: OrderBy/OrderByDescending
                orderedQuery = isDescending
                    ? source.OrderByDescending(typedExpression)
                    : source.OrderBy(typedExpression);
            }
            else
            {
                // Subsequent sorts: ThenBy/ThenByDescending
                orderedQuery = isDescending
                    ? orderedQuery.ThenByDescending(typedExpression)
                    : orderedQuery.ThenBy(typedExpression);
            }
        }

        return orderedQuery ?? source;
    }

    /// <summary>
    /// Parse sort query string into field names and directions
    /// </summary>
    private static List<(string fieldName, bool isDescending)> ParseSortFields(string sortQuery)
    {
        var result = new List<(string, bool)>();
        var fields = sortQuery.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field))
                continue;

            var isDescending = field.StartsWith('-');
            var fieldName = isDescending ? field[1..] : field;

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                result.Add((fieldName, isDescending));
            }
        }

        return result;
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

    /// <summary>
    /// Build expression for a specific operator and value
    /// Supports: eq, neq, gt, gte, lt, lte, in, contains, startswith, endswith
    /// </summary>
    /// <param name="member">Member expression to apply operator to</param>
    /// <param name="op">Operator name (lowercase)</param>
    /// <param name="value">Raw string value to compare</param>
    /// <returns>Boolean expression or null if invalid</returns>
    private static Expression? BuildExpressionForOperator(Expression member, string op, string value)
    {
        // Handle null value comparisons
        if (IsNullValue(value))
            return BuildNullComparison(member, op);

        // Handle 'in' operator (comma-separated list)
        if (op == "in")
            return BuildInExpression(member, value);

        // Handle string operations
        if (IsStringOperation(op))
            return BuildStringOperation(member, op, value);

        // Handle numeric/date/enum comparisons
        return BuildComparisonExpression(member, op, value);
    }

    /// <summary>
    /// Check if value represents null
    /// </summary>
    private static bool IsNullValue(string value)
        => string.Equals(value, "null", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Build null comparison expressions
    /// </summary>
    private static Expression? BuildNullComparison(Expression member, string op)
    {
        var nullConstant = Expression.Constant(null, member.Type);
        
        return op switch
        {
            "eq" => Expression.Equal(member, nullConstant),
            "neq" or "ne" or "notnull" => Expression.NotEqual(member, nullConstant),
            _ => null
        };
    }

    /// <summary>
    /// Build 'in' operator expression for list matching
    /// </summary>
    private static Expression? BuildInExpression(Expression member, string value)
    {
        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Expression? combined = null;

        foreach (var item in items)
        {
            var parsedValue = ParseRawToType(item, member.Type);
            if (parsedValue == null)
                continue;

            var equalExpression = Expression.Equal(
                member,
                Expression.Constant(parsedValue, member.Type));

            combined = combined == null
                ? equalExpression
                : Expression.OrElse(combined, equalExpression);
        }

        return combined;
    }

    /// <summary>
    /// Check if operator is a string operation
    /// </summary>
    private static bool IsStringOperation(string op)
        => op is "contains" or "startswith" or "endswith";

    /// <summary>
    /// Build string operation expressions (contains, startswith, endswith)
    /// </summary>
    private static Expression? BuildStringOperation(Expression member, string op, string value)
    {
        // Ensure we have a string expression
        var stringExpression = member.Type == typeof(string)
            ? member
            : Expression.Call(member, nameof(ToString), Type.EmptyTypes);

        var methodName = op switch
        {
            "contains" => nameof(string.Contains),
            "startswith" => nameof(string.StartsWith),
            "endswith" => nameof(string.EndsWith),
            _ => null
        };

        if (methodName == null)
            return null;

        var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });
        if (method == null)
            return null;

        return Expression.Call(stringExpression, method, Expression.Constant(value));
    }

    /// <summary>
    /// Build comparison expressions (eq, neq, gt, gte, lt, lte)
    /// </summary>
    private static Expression? BuildComparisonExpression(Expression member, string op, string value)
    {
        var parsedValue = ParseRawToType(value, member.Type);
        if (parsedValue == null)
            return null;

        var constantExpression = Expression.Constant(parsedValue, member.Type);

        return op switch
        {
            "eq" => Expression.Equal(member, constantExpression),
            "neq" or "ne" => Expression.NotEqual(member, constantExpression),
            "gt" => Expression.GreaterThan(member, constantExpression),
            "gte" => Expression.GreaterThanOrEqual(member, constantExpression),
            "lt" => Expression.LessThan(member, constantExpression),
            "lte" => Expression.LessThanOrEqual(member, constantExpression),
            _ => null
        };
    }

    /// <summary>
    /// Parse string value to target type with culture-invariant parsing
    /// Supports: string, int, long, decimal, double, bool, DateTime, enums
    /// </summary>
    /// <param name="value">String value to parse</param>
    /// <param name="targetType">Target type to convert to</param>
    /// <returns>Parsed value or null if parsing fails</returns>
    private static object? ParseRawToType(string? value, Type targetType)
    {
        if (value == null || IsNullValue(value))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            return underlyingType switch
            {
                _ when underlyingType == typeof(string) => value,
                _ when underlyingType == typeof(int) => int.Parse(value, CultureInfo.InvariantCulture),
                _ when underlyingType == typeof(long) => long.Parse(value, CultureInfo.InvariantCulture),
                _ when underlyingType == typeof(decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
                _ when underlyingType == typeof(double) => double.Parse(value, CultureInfo.InvariantCulture),
                _ when underlyingType == typeof(float) => float.Parse(value, CultureInfo.InvariantCulture),
                _ when underlyingType == typeof(bool) => bool.Parse(value),
                _ when underlyingType == typeof(DateTime) => DateTime.Parse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                _ when underlyingType == typeof(DateTimeOffset) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture),
                _ when underlyingType == typeof(Guid) => Guid.Parse(value),
                _ when underlyingType.IsEnum => Enum.Parse(underlyingType, value, ignoreCase: true),
                _ => Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture)
            };
        }
        catch
        {
            // Return null for any parsing failures
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
