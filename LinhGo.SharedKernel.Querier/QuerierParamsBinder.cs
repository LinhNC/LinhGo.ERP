using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LinhGo.SharedKernel.Querier;

/// <summary>
/// Custom model binder for QuerierParams that parses query string parameters
/// and sorts filters/fields alphabetically for consistent ordering
/// </summary>
internal class QuerierParamsBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var query = bindingContext.HttpContext.Request.Query;
        
        var searchParams = new QuerierParams
        {
            Q = GetQueryValue(query, QuerierConstants.QuerySearchKey),
            Sorts = GetQueryValue(query, QuerierConstants.QuerySortKey),
            Includes = GetQueryValue(query, QuerierConstants.QueryIncludeKey),
            Page = GetIntValue(query, QuerierConstants.QueryPageKey, defaultValue: QuerierConstants.DefaultPageNumber),
            PageSize = GetIntValue(query, QuerierConstants.QueryPageSizeKey, defaultValue: QuerierConstants.DefaultPageSize),
            Fields = ParseAndSortFields(GetQueryValue(query, QuerierConstants.QueryFieldsKey))
        };

        // Parse and sort filter entries alphabetically
        ParseFilters(query, searchParams, bindingContext);

        bindingContext.Result = ModelBindingResult.Success(searchParams);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Parse filter[field] or filter[field][operator] entries and add them sorted alphabetically
    /// </summary>
    private static void ParseFilters(IQueryCollection query, QuerierParams searchParams, ModelBindingContext bindingContext)
    {
        // Collect all filter entries first for sorting
        var filterEntries = new List<FilterEntry>(capacity: 10);

        foreach (var queryParam in query)
        {
            if (!TryParseFilterKey(queryParam.Key, out var field, out var op))
                continue;

            filterEntries.Add(new FilterEntry(field, op, queryParam.Value.ToString()));
        }

        // Sort alphabetically by field name, then by operator
        filterEntries.Sort((a, b) =>
        {
            var fieldComparison = string.Compare(a.Field, b.Field, StringComparison.OrdinalIgnoreCase);
            return fieldComparison != 0 
                ? fieldComparison 
                : string.Compare(a.Operator, b.Operator, StringComparison.OrdinalIgnoreCase);
        });

        // Add sorted filters to the params
        foreach (var entry in filterEntries)
        {
            if (!bindingContext.ModelState.IsValid) 
                break;
            if (searchParams.Filters is null)
            {
                break;
            }

            if (!searchParams.Filters.TryGetValue(entry.Field, out var operatorDict))
            {
                operatorDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                searchParams.Filters[entry.Field] = operatorDict;
            }

            operatorDict[entry.Operator] = entry.Value;
        }
    }

    /// <summary>
    /// Try to parse filter key into field name and operator
    /// Supports: filter[field] or filter[field][operator]
    /// </summary>
    private static bool TryParseFilterKey(string key, out string field, out string op)
    {
        field = string.Empty;
        op = QuerierConstants.DefaultOperator;

        if (key.Length < QuerierConstants.MinFilterKeyLength || !key.AsSpan().StartsWith(QuerierConstants.QueryFilterKeyPrefix))
            return false;

        var span = key.AsSpan(QuerierConstants.QueryFilterKeyPrefix.Length);
        var closingBracketIndex = span.IndexOf(']');
        
        if (closingBracketIndex <= 0)
            return false;

        field = span.Slice(0, closingBracketIndex).ToString();
        var remainder = span.Slice(closingBracketIndex + 1);

        // Check for [operator] after field
        if (!remainder.IsEmpty)
        {
            if (remainder.Length > 2 && remainder[0] == '[' && remainder[^1] == ']')
            {
                op = remainder.Slice(1, remainder.Length - 2).ToString();
            }
            else
            {
                return false; // Malformed
            }
        }

        return true;
    }

    /// <summary>
    /// Parse comma-separated fields and return them sorted alphabetically as a dictionary
    /// Removes duplicates and trims whitespace
    /// </summary>
    private static Dictionary<string, string>? ParseAndSortFields(string? fieldsString)
    {
        if (string.IsNullOrWhiteSpace(fieldsString))
            return null;

        var fieldArray = fieldsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (fieldArray.Length == 0)
            return null;

        // Use dictionary for O(1) lookup and deduplication
        var fieldsDictionary = new Dictionary<string, string>(
            capacity: fieldArray.Length,
            comparer: StringComparer.OrdinalIgnoreCase);

        foreach (var field in fieldArray)
        {
            fieldsDictionary[field] = field;
        }

        return fieldsDictionary;
    }

    /// <summary>
    /// Get a single query string value
    /// </summary>
    private static string? GetQueryValue(IQueryCollection query, string key)
        => query.TryGetValue(key, out var value) ? value.ToString() : null;

    /// <summary>
    /// Get an integer query string value with a default fallback
    /// </summary>
    private static int GetIntValue(IQueryCollection query, string key, int defaultValue)
        => query.TryGetValue(key, out var value) && int.TryParse(value, out var intValue) 
            ? intValue 
            : defaultValue;

    /// <summary>
    /// Internal struct to hold filter entry data for sorting
    /// </summary>
    private readonly record struct FilterEntry(string Field, string Operator, string Value);
}