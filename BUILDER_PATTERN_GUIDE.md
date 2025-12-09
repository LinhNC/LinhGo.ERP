# ✅ Correct Builder Pattern Implementation Guide

## Overview

The `SearchBuilder<T>` class now follows proper Builder pattern best practices with a fluent API for constructing and executing search queries.

## Key Improvements Made

### 1. ✅ **Remove Nullable SearchQueryEngine**
**Before (Incorrect):**
```csharp
private SearchQueryEngine<T>? _searchQueryEngine;  // ❌ Nullable, unnecessary

public SearchBuilder<T> WithSource(IQueryable<T> queryable)
{
    _searchQueryEngine?.SetSource(queryable);  // ❌ Null-conditional operator
    return this;
}
```

**After (Correct):**
```csharp
private readonly SearchQueryEngine<T> _searchQueryEngine;  // ✅ Not nullable, readonly

public SearchBuilder<T> WithSource(IQueryable<T> queryable)
{
    ThrowIfAlreadyBuilt();
    ArgumentNullException.ThrowIfNull(queryable);
    
    _searchQueryEngine.SetSource(queryable);  // ✅ Direct call
    return this;
}
```

**Why:**
- SearchQueryEngine is always initialized in constructor
- No need for nullable reference
- Avoids unnecessary null checks
- Makes code cleaner and safer

### 2. ✅ **Single-Use Builder with Build Protection**
```csharp
private bool _isBuilt;

private void ThrowIfAlreadyBuilt()
{
    if (_isBuilt)
    {
        throw new InvalidOperationException(
            "This SearchBuilder has already been built and executed. " +
            "Create a new instance for additional searches.");
    }
}
```

**Why:**
- Prevents reusing the same builder instance
- Ensures predictable behavior
- Follows Builder pattern best practices
- Clear error messages for misuse

### 3. ✅ **Async Build with Direct Result Return**
**Before (Incorrect):**
```csharp
public SearchBuilder<T> Build(CancellationToken ctx = default)  // ❌ Not async
{
    _searchQueryEngine?.ExecuteAsync(ctx);  // ❌ Fire and forget
    return this;
}

public PagedResult<T> GetResult()  // ❌ Separate method to get result
{
    return _searchQueryEngine?.Result() ?? throw new InvalidOperationException(...);
}
```

**After (Correct):**
```csharp
public async Task<PagedResult<T>> BuildAsync(CancellationToken cancellationToken = default)
{
    ThrowIfAlreadyBuilt();
    _isBuilt = true;
    
    await _searchQueryEngine.ExecuteAsync(cancellationToken);
    
    return _searchQueryEngine.Result() 
        ?? throw new InvalidOperationException("Search query execution did not produce a result.");
}
```

**Why:**
- Properly awaits async operation
- Returns result directly (no separate GetResult method)
- Terminal operation pattern
- Cleaner API

### 4. ✅ **Argument Validation**
```csharp
public SearchBuilder<T> WithSource(IQueryable<T> queryable)
{
    ThrowIfAlreadyBuilt();
    ArgumentNullException.ThrowIfNull(queryable);  // ✅ Validate arguments
    
    _searchQueryEngine.SetSource(queryable);
    return this;
}
```

**Why:**
- Fail fast with clear error messages
- Prevents invalid state
- Follows defensive programming

### 5. ✅ **Sealed Class**
```csharp
public sealed class SearchBuilder<T> where T : class
```

**Why:**
- Builder shouldn't be inherited
- Better performance (no virtual dispatch)
- Clear intent

### 6. ✅ **Comprehensive XML Documentation**
```csharp
/// <summary>
/// Fluent Builder for constructing and executing search queries
/// Follows the Builder pattern with a fluent API for better readability
/// </summary>
/// <typeparam name="T">Entity type to search</typeparam>
public sealed class SearchBuilder<T> where T : class
```

**Why:**
- IntelliSense documentation
- Better developer experience
- Self-documenting code

## Usage Examples

### Example 1: Basic Search (Minimum Configuration)
```csharp
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .BuildAsync(cancellationToken);
```

### Example 2: Search with Custom Selector
```csharp
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .WithSelector(c => c)  // Identity selector
    .BuildAsync(cancellationToken);
```

### Example 3: Search with Includes
```csharp
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .WithIncludeSettings(
        includeIsAllowed: path => path == "Settings",
        includeApplier: (query, path) => query.Include(path)
    )
    .BuildAsync(cancellationToken);
```

### Example 4: In Repository Method
```csharp
public async Task<PagedResult<Company>> SearchAsync(
    SearchQueryParams queries, 
    CancellationToken cancellationToken = default)
{
    return await new SearchBuilder<Company>()
        .WithSource(DbSet.AsNoTracking())
        .WithQueryParams(queries)
        .WithFilterMapping(FilterableFields)
        .WithSortMapping(SortableFields)
        .BuildAsync(cancellationToken);
}
```

### Example 5: Error Handling
```csharp
try
{
    var result = await new SearchBuilder<Company>()
        .WithSource(dbContext.Companies.AsNoTracking())
        .WithQueryParams(searchParams)
        .WithFilterMapping(FilterableFields)
        .WithSortMapping(SortableFields)
        .BuildAsync(cancellationToken);
        
    return Ok(result);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("already been built"))
{
    // Builder reuse attempted
    logger.LogError(ex, "Builder reuse attempted");
    return StatusCode(500, "Internal server error");
}
catch (ArgumentNullException ex)
{
    // Missing required argument
    logger.LogError(ex, "Missing required argument");
    return BadRequest(ex.Message);
}
```

## Common Mistakes to Avoid

### ❌ Mistake 1: Reusing Builder Instance
```csharp
// ❌ WRONG - Reusing builder
var builder = new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams);

var result1 = await builder.BuildAsync();  // OK
var result2 = await builder.BuildAsync();  // ❌ Throws InvalidOperationException
```

**✅ Correct:**
```csharp
// Create new builder for each search
var result1 = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams1)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .BuildAsync();

var result2 = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams2)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .BuildAsync();
```

### ❌ Mistake 2: Not Awaiting BuildAsync
```csharp
// ❌ WRONG - Not awaiting
var task = new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .BuildAsync();  // Returns Task<PagedResult<T>>

// ... later ...
var result = task.Result;  // ❌ Blocking call, deadlock risk
```

**✅ Correct:**
```csharp
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .BuildAsync();  // Properly await
```

### ❌ Mistake 3: Missing Required Configuration
```csharp
// ❌ WRONG - Missing filter and sort mappings
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .BuildAsync();  // Will throw ArgumentNullException
```

**✅ Correct:**
```csharp
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)  // Required
    .WithSortMapping(SortableFields)      // Required
    .BuildAsync();
```

### ❌ Mistake 4: Calling Methods After Build
```csharp
// ❌ WRONG - Modifying after build
var builder = new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields);

var result = await builder.BuildAsync();

builder.WithSelector(c => c);  // ❌ Throws InvalidOperationException
```

**✅ Correct:**
```csharp
var result = await new SearchBuilder<Company>()
    .WithSource(dbContext.Companies.AsNoTracking())
    .WithQueryParams(searchParams)
    .WithFilterMapping(FilterableFields)
    .WithSortMapping(SortableFields)
    .WithSelector(c => c)  // Set before build
    .BuildAsync();
```

## Builder Pattern Best Practices Applied

### ✅ 1. Immutable After Build
Once `BuildAsync()` is called, the builder cannot be modified or reused.

### ✅ 2. Fluent API
All configuration methods return `this` for method chaining.

### ✅ 3. Validation
Arguments are validated before being passed to the engine.

### ✅ 4. Single Responsibility
Builder only builds, engine only executes.

### ✅ 5. Terminal Operation
`BuildAsync()` is the terminal operation that returns the final result.

### ✅ 6. Clear Error Messages
Exceptions provide clear guidance on what went wrong.

### ✅ 7. Async/Await Support
Properly supports asynchronous operations.

### ✅ 8. Type Safety
Generic type parameter ensures type safety throughout.

## Performance Considerations

### Memory Allocation
```csharp
// ✅ Good - Single allocation per search
var result = await new SearchBuilder<Company>()
    .WithSource(...)
    .WithQueryParams(...)
    .BuildAsync();
```

### Async Context
```csharp
// ✅ Good - Proper async/await
public async Task<PagedResult<Company>> SearchAsync(...)
{
    return await new SearchBuilder<Company>()
        .WithSource(...)
        .BuildAsync(cancellationToken);
}
```

### Cancellation Support
```csharp
// ✅ Good - Respects cancellation token
var result = await new SearchBuilder<Company>()
    .WithSource(...)
    .WithQueryParams(...)
    .BuildAsync(cancellationToken);  // Pass token through
```

## Testing Examples

### Unit Test Example
```csharp
[Fact]
public async Task SearchBuilder_WithValidConfig_ReturnsResults()
{
    // Arrange
    var companies = new List<Company>
    {
        new Company { Id = 1, Name = "Test Co", IsActive = true }
    }.AsQueryable();
    
    var searchParams = new SearchQueryParams { Page = 1, PageSize = 10 };
    
    // Act
    var result = await new SearchBuilder<Company>()
        .WithSource(companies)
        .WithQueryParams(searchParams)
        .WithFilterMapping(GetFilterMap())
        .WithSortMapping(GetSortMap())
        .BuildAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Items);
}

[Fact]
public async Task SearchBuilder_WhenBuiltTwice_ThrowsException()
{
    // Arrange
    var builder = new SearchBuilder<Company>()
        .WithSource(GetQueryable())
        .WithQueryParams(new SearchQueryParams())
        .WithFilterMapping(GetFilterMap())
        .WithSortMapping(GetSortMap());
    
    await builder.BuildAsync();
    
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => builder.BuildAsync()
    );
}
```

## Summary

✅ **Removed unnecessary nullable operators** - Cleaner code  
✅ **Added single-use enforcement** - Prevents misuse  
✅ **Made BuildAsync async and return result** - Better API  
✅ **Added argument validation** - Fail fast  
✅ **Added comprehensive documentation** - Better DX  
✅ **Sealed the class** - Better performance  
✅ **Clear error messages** - Easier debugging  

**The SearchBuilder now follows proper Builder pattern best practices!** 🎉

