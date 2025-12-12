using System.Linq.Expressions;
using LinhGo.SharedKernel.Result;

namespace LinhGo.SharedKernel.Querier;

/// <summary>
/// Fluent Builder for constructing and executing search queries
/// </summary>
/// <typeparam name="T">Entity type to search</typeparam>
public sealed class QuerierBuilder<T> where T : class
{
    private readonly QuerierEngine<T> _queryEngine;
    private bool _isBuilt;
    
    /// <summary>
    /// Initialize a new QuerierBuilder instance
    /// </summary>
    public QuerierBuilder()
    {
        _queryEngine = new QuerierEngine<T>();
    }
    
    /// <summary>
    /// Set the source IQueryable to search against
    /// </summary>
    /// <param name="queryable">Source query (e.g., DbSet.AsNoTracking())</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public QuerierBuilder<T> WithSource(IQueryable<T> queryable)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(queryable);
        
        _queryEngine.SetSource(queryable);
        return this;
    }
    
    /// <summary>
    /// Set the query parameters (filters, sorts, pagination)
    /// </summary>
    /// <param name="searchParams">Search query parameters from API request</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public QuerierBuilder<T> WithQueryParams(QuerierParams querierParams)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(querierParams);
        
        _queryEngine.SetQueryParams(querierParams);
        return this;
    }

    /// <summary>
    /// Set the selector for projection (optional, defaults to identity selector)
    /// </summary>
    /// <param name="selector">Expression to project entity</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public QuerierBuilder<T> WithSelector(Expression<Func<T, T>> selector)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(selector);
        
        _queryEngine.SetFieldSelectors(selector);
        return this;
    }
    
    /// <summary>
    /// Set the filter field mappings for dynamic filtering
    /// </summary>
    /// <param name="filterMappingFields">Dictionary mapping field names to property expressions</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public QuerierBuilder<T> WithFilterMappingFields(IReadOnlyDictionary<string, Expression<Func<T, object>>> filterMappingFields)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(filterMappingFields);
        
        _queryEngine.SetFilterMappingFields(filterMappingFields);
        return this;
    }
    
    /// <summary>
    /// Set the sort field mappings for dynamic sorting
    /// </summary>
    /// <param name="sortMappingFields">Dictionary mapping field names to sort expressions</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public QuerierBuilder<T> WithSortMappingFields(IReadOnlyDictionary<string, LambdaExpression> sortMappingFields)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(sortMappingFields);
        
        _queryEngine.SetSortMappingFields(sortMappingFields);
        return this;
    }
    
    /// <summary>
    /// Set the include settings for eager loading related entities (optional)
    /// </summary>
    /// <param name="includeIsAllowed">Function to validate if an include is allowed</param>
    /// <param name="includeApplier">Function to apply the include to the query</param>
    /// <returns>Builder instance for fluent chaining</returns>
    public QuerierBuilder<T> WithIncludeSettings(
        Func<string, bool>? includeIsAllowed,
        Func<IQueryable<T>, string?, IQueryable<T>>? includeApplier)
    {
        ThrowIfAlreadyBuilt();
        
        _queryEngine.SetIncludeSettings(includeIsAllowed, includeApplier);
        return this;
    }
    
    /// <summary>
    /// Set the full-text search field
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public QuerierBuilder<T> WithFullTextSearchSettings(
        Expression<Func<T, object>> expression)
    {
        ThrowIfAlreadyBuilt();
        ArgumentNullException.ThrowIfNull(expression);
        
        _queryEngine.SetFullTextSearchField(expression);
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
        
        await _queryEngine.ExecuteAsync(cancellationToken);
        
        return _queryEngine.Result() 
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