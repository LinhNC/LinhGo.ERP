using System.Linq.Expressions;
using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Application.Common.SearchBuilders;

/// <summary>
/// Fluent Builder for constructing and executing search queries
/// Follows the Builder pattern with a fluent API for better readability
/// </summary>
/// <typeparam name="T">Entity type to search</typeparam>
public sealed class SearchBuilder<T> where T : class
{
    private readonly SearchQueryEngine<T> _searchQueryEngine;
    private bool _isBuilt;
    
    /// <summary>
    /// Initialize a new SearchBuilder instance
    /// </summary>
    public SearchBuilder()
    {
        _searchQueryEngine = new SearchQueryEngine<T>();
    }
    
    /// <summary>
    /// Set the source IQueryable to search against
    /// </summary>
    /// <param name="queryable">Source query (e.g., DbSet.AsNoTracking())</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public SearchBuilder<T> WithSource(IQueryable<T> queryable)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(queryable);
        
        _searchQueryEngine.SetSource(queryable);
        return this;
    }
    
    /// <summary>
    /// Set the query parameters (filters, sorts, pagination)
    /// </summary>
    /// <param name="searchParams">Search query parameters from API request</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public SearchBuilder<T> WithQueryParams(SearchQueryParams searchParams)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(searchParams);
        
        _searchQueryEngine.SetQueryParams(searchParams);
        return this;
    }

    /// <summary>
    /// Set the selector for projection (optional, defaults to identity selector)
    /// </summary>
    /// <param name="selector">Expression to project entity</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public SearchBuilder<T> WithSelector(Expression<Func<T, T>> selector)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(selector);
        
        _searchQueryEngine.SetSelector(selector);
        return this;
    }
    
    /// <summary>
    /// Set the filter field mappings for dynamic filtering
    /// </summary>
    /// <param name="filterMap">Dictionary mapping field names to property expressions</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public SearchBuilder<T> WithFilterMapping(IReadOnlyDictionary<string, Expression<Func<T, object>>> filterMap)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(filterMap);
        
        _searchQueryEngine.SetFilterMapping(filterMap);
        return this;
    }
    
    /// <summary>
    /// Set the sort field mappings for dynamic sorting
    /// </summary>
    /// <param name="sortMap">Dictionary mapping field names to sort expressions</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public SearchBuilder<T> WithSortMapping(IReadOnlyDictionary<string, LambdaExpression> sortMap)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(sortMap);
        
        _searchQueryEngine.SetSortMapping(sortMap);
        return this;
    }
    
    /// <summary>
    /// Set the include settings for eager loading related entities (optional)
    /// </summary>
    /// <param name="includeIsAllowed">Function to validate if an include is allowed</param>
    /// <param name="includeApplier">Function to apply the include to the query</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public SearchBuilder<T> WithIncludeSettings(
        Func<string, bool>? includeIsAllowed,
        Func<IQueryable<T>, string?, IQueryable<T>>? includeApplier)
    {
        ThrowIfAlreadyBuilt();
        
        _searchQueryEngine.SetIncludeSettings(includeIsAllowed, includeApplier);
        return this;
    }
    
    /// <summary>
    /// Build and execute the search query asynchronously
    /// This is the terminal operation that executes the query and returns results
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated search results</returns>
    /// <exception cref="InvalidOperationException">Thrown if Build is called multiple times</exception>
    public async Task<PagedResult<T>> BuildAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfAlreadyBuilt();
        _isBuilt = true;
        
        await _searchQueryEngine.ExecuteAsync(cancellationToken);
        
        return _searchQueryEngine.Result() 
            ?? throw new InvalidOperationException("Search query execution did not produce a result.");
    }
    
    /// <summary>
    /// Ensure the builder hasn't been built yet (builder should be single-use)
    /// </summary>
    private void ThrowIfAlreadyBuilt()
    {
        if (_isBuilt)
        {
            throw new InvalidOperationException(
                "This SearchBuilder has already been built and executed. " +
                "Create a new instance for additional searches.");
        }
    }
}