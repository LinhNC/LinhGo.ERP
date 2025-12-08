# Company Search and Pagination Guide

## Overview

This guide covers the advanced search and pagination functionality for the Company module in LinhGo.ERP. The implementation follows best practices for performance, maintainability, and scalability.

## Features

### 1. **Advanced Search**
- Search by company name, code, or email (case-insensitive)
- Filter by multiple criteria simultaneously
- Supports partial text matching

### 2. **Multiple Filters**
- **Currency**: Filter by currency code (USD, EUR, VND, etc.)
- **Country**: Filter by country name
- **Industry**: Filter by industry sector
- **Active Status**: Filter active/inactive companies
- **City**: Filter by city location
- **Subscription Plan**: Filter by subscription tier

### 3. **Pagination**
- Configurable page size (1-100 items per page)
- Page-based navigation
- Total count and page metadata included in response
- Efficient query execution with `Skip()` and `Take()`

### 4. **Sorting**
- Sort by multiple fields: Name, Code, CreatedAt, UpdatedAt, Currency, Country
- Support for ascending and descending order
- Default sort by Name (ascending)

## API Endpoint

### Search Companies

```http
GET /api/v1/companies/search
```

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `searchTerm` | string | No | null | Search in name, code, or email |
| `currency` | string | No | null | Filter by currency code |
| `country` | string | No | null | Filter by country |
| `industry` | string | No | null | Filter by industry |
| `isActive` | boolean | No | null | Filter by active status |
| `city` | string | No | null | Filter by city |
| `subscriptionPlan` | string | No | null | Filter by subscription plan |
| `page` | integer | No | 1 | Page number (1-based) |
| `pageSize` | integer | No | 10 | Items per page (max 100) |
| `sortBy` | string | No | "Name" | Sort field |
| `sortDirection` | string | No | "asc" | Sort direction (asc/desc) |

#### Example Requests

**Basic Search:**
```http
GET /api/v1/companies/search?searchTerm=tech&page=1&pageSize=20
```

**Advanced Filter:**
```http
GET /api/v1/companies/search?currency=USD&country=USA&isActive=true&sortBy=CreatedAt&sortDirection=desc
```

**Combined Search and Filter:**
```http
GET /api/v1/companies/search?searchTerm=software&industry=Technology&city=San Francisco&page=1&pageSize=50
```

#### Response Format

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "COMP001",
      "name": "Tech Company Inc.",
      "email": "contact@techcompany.com",
      "currency": "USD",
      "country": "USA",
      "city": "San Francisco",
      "industry": "Technology",
      "isActive": true,
      "version": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-15T00:00:00Z"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Implementation Details

### Architecture Layers

#### 1. **DTO Layer** (`CompanySearchDto`)
- Input validation and normalization
- Parameter sanitization
- Default value handling
- Bounds checking (page size limits)

```csharp
public class CompanySearchDto
{
    public string? SearchTerm { get; set; }
    public string? Currency { get; set; }
    public string? Country { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public void Normalize()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100;
        // ... additional normalization
    }
}
```

#### 2. **Repository Layer** (`CompanyRepository.SearchAsync`)
- **Query Building**: Constructs IQueryable with filters
- **Performance Optimization**:
  - Uses `AsQueryable()` for deferred execution
  - Applies filters before pagination
  - Single database round-trip for count and items
  - Efficient indexing on filterable columns recommended

```csharp
public async Task<(IEnumerable<Company> Items, int TotalCount)> SearchAsync(...)
{
    var query = DbSet.AsQueryable();
    
    // Apply filters
    if (!string.IsNullOrWhiteSpace(searchTerm))
        query = query.Where(c => c.Name.Contains(searchTerm) || ...);
    
    // Get total count before pagination
    var totalCount = await query.CountAsync(cancellationToken);
    
    // Apply sorting and pagination
    query = ApplySorting(query, sortBy, sortDirection);
    var items = await query.Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync(cancellationToken);
    
    return (items, totalCount);
}
```

#### 3. **Service Layer** (`CompanyService.SearchAsync`)
- Business logic validation
- DTO mapping with AutoMapper
- Error handling and logging
- Result pattern for error management

```csharp
public async Task<Result<PagedResult<CompanyDto>>> SearchAsync(CompanySearchDto searchDto)
{
    try
    {
        searchDto.Normalize();
        logger.LogInformation("Searching companies...");
        
        var (items, totalCount) = await companyRepository.SearchAsync(...);
        var companyDtos = mapper.Map<IEnumerable<CompanyDto>>(items);
        
        var pagedResult = new PagedResult<CompanyDto>
        {
            Items = companyDtos,
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize
        };
        
        return pagedResult;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error searching companies");
        return Error.WithFailureCode(CompanyErrors.GetAllFailed);
    }
}
```

#### 4. **Controller Layer** (`CompaniesController.Search`)
- Route definition
- OpenAPI documentation
- Query parameter binding
- HTTP status code mapping

## Best Practices Implemented

### 1. **Performance Optimization**

✅ **Deferred Execution**: Uses `IQueryable` to build queries without immediate execution
✅ **Efficient Pagination**: `Skip()` and `Take()` translate to SQL `LIMIT` and `OFFSET`
✅ **Single Query for Count**: Counts before pagination to avoid loading all records
✅ **Case-Insensitive Search**: Uses `ToLower()` for database-supported case-insensitive matching
✅ **Index-Friendly Queries**: Filter conditions support database indexes

### 2. **Input Validation**

✅ **Normalization**: `Normalize()` method ensures valid input
✅ **Bounds Checking**: Page size limited to 1-100
✅ **Null Safety**: Handles null/empty search terms gracefully
✅ **Trim Whitespace**: Removes leading/trailing spaces

### 3. **Maintainability**

✅ **Separation of Concerns**: Clear responsibility per layer
✅ **SOLID Principles**: Single responsibility, dependency injection
✅ **DRY**: Reusable pagination and error handling patterns
✅ **Testable**: Each layer can be unit tested independently

### 4. **Security**

✅ **SQL Injection Prevention**: Entity Framework parameterized queries
✅ **Rate Limiting**: Page size maximum prevents abuse
✅ **Input Sanitization**: Trim and normalize user input

### 5. **API Design**

✅ **RESTful Conventions**: Proper HTTP methods and status codes
✅ **OpenAPI Documentation**: Auto-generated API documentation
✅ **Consistent Response Format**: Uses `PagedResult<T>` pattern
✅ **Metadata Included**: Total count, page info for client-side pagination

## Database Performance Considerations

### Recommended Indexes

For optimal search performance, consider adding these indexes:

```sql
-- Index for name search (case-insensitive)
CREATE INDEX idx_companies_name_lower ON companies (LOWER(name));

-- Index for code search (case-insensitive)
CREATE INDEX idx_companies_code_lower ON companies (LOWER(code));

-- Index for email search (case-insensitive)
CREATE INDEX idx_companies_email_lower ON companies (LOWER(email)) WHERE email IS NOT NULL;

-- Composite indexes for common filter combinations
CREATE INDEX idx_companies_currency_country ON companies (currency, country);
CREATE INDEX idx_companies_active_created ON companies (is_active, created_at DESC);
CREATE INDEX idx_companies_industry ON companies (industry) WHERE industry IS NOT NULL;
```

### Query Performance Tips

1. **Use specific filters**: More specific searches = faster queries
2. **Limit page size**: Smaller pages = faster response times
3. **Monitor slow queries**: Use PostgreSQL's `pg_stat_statements`
4. **Consider full-text search**: For complex text searches, use PostgreSQL full-text search features

## Testing Examples

### Unit Test Example

```csharp
[Fact]
public async Task SearchAsync_WithFilters_ReturnsFilteredResults()
{
    // Arrange
    var searchDto = new CompanySearchDto
    {
        SearchTerm = "tech",
        Currency = "USD",
        IsActive = true,
        Page = 1,
        PageSize = 10
    };

    // Act
    var result = await _companyService.SearchAsync(searchDto);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.All(result.Value.Items, c => 
    {
        Assert.Contains("tech", c.Name.ToLower());
        Assert.Equal("USD", c.Currency);
        Assert.True(c.IsActive);
    });
}
```

### Integration Test Example

```csharp
[Fact]
public async Task Search_WithPagination_ReturnsCorrectPage()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync(
        "/api/v1/companies/search?page=2&pageSize=10&sortBy=Name");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<PagedResult<CompanyDto>>();
    
    Assert.NotNull(result);
    Assert.Equal(2, result.Page);
    Assert.True(result.Items.Count() <= 10);
}
```

## Frontend Integration Example

### JavaScript/TypeScript Example

```typescript
interface CompanySearchParams {
  searchTerm?: string;
  currency?: string;
  country?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

async function searchCompanies(params: CompanySearchParams): Promise<PagedResult<Company>> {
  const queryString = new URLSearchParams(
    Object.entries(params)
      .filter(([_, value]) => value !== undefined)
      .map(([key, value]) => [key, String(value)])
  ).toString();
  
  const response = await fetch(`/api/v1/companies/search?${queryString}`);
  
  if (!response.ok) {
    throw new Error('Search failed');
  }
  
  return await response.json();
}

// Usage
const results = await searchCompanies({
  searchTerm: 'tech',
  currency: 'USD',
  page: 1,
  pageSize: 20,
  sortBy: 'Name',
  sortDirection: 'asc'
});

console.log(`Found ${results.totalCount} companies`);
console.log(`Page ${results.page} of ${results.totalPages}`);
```

## Troubleshooting

### Common Issues

1. **Slow queries with large datasets**
   - **Solution**: Add appropriate database indexes (see above)
   - Reduce page size
   - Use more specific filters

2. **Case-sensitive search not working**
   - **Solution**: The implementation uses `ToLower()` for case-insensitive search
   - Database collation settings may affect behavior

3. **Pagination showing incorrect total count**
   - **Solution**: Ensure `CountAsync()` is called before pagination filters

4. **Sort field not working**
   - **Solution**: Check that sortBy field name matches entity property name (case-insensitive)

## Future Enhancements

- [ ] Full-text search integration
- [ ] Saved search filters
- [ ] Export search results (CSV, Excel)
- [ ] Search result caching
- [ ] Advanced analytics on search patterns
- [ ] Faceted search (show available filter options with counts)

## Related Documentation

- [API Error Response Format](./API_ERROR_RESPONSE_FORMAT.md)
- [Pagination Best Practices](./API_LAYER_SUMMARY.md)
- [Database Configuration](./DATABASE_CONFIGURATION_SUMMARY.md)

## Summary

The company search and pagination feature provides a robust, performant, and maintainable solution for querying company data. It follows industry best practices and can handle large datasets efficiently.

**Key Benefits:**
- ⚡ Fast query execution with proper indexing
- 🔍 Flexible search and filtering options
- 📄 Efficient pagination for large datasets
- 🛡️ Secure with SQL injection prevention
- 📚 Well-documented with OpenAPI
- 🧪 Testable architecture
- 🔧 Easy to extend with new filters

