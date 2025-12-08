# Multi-Field Sorting Implementation Guide

## Overview

This guide explains the best practices implementation for multi-field sorting in the Company Search API. The implementation supports sorting by multiple fields with individual sort directions (ascending/descending) for each field.

## Key Features

### ✅ **Best Practices Implemented**

1. **Type-Safe Sorting**
   - Immutable `SortSpecification` record for thread-safety
   - Validation of field names and directions
   - Clear error messages for invalid inputs

2. **Multiple Sort Fields**
   - Support up to 5 sort fields simultaneously
   - Each field can have independent sort direction (asc/desc)
   - Duplicate fields automatically removed (first occurrence kept)

3. **Flexible Input Formats**
   - Query string format: `?sort=Name:asc&sort=CreatedAt:desc`
   - Field only: `?sort=Name` (defaults to ascending)
   - JSON body format for complex requests

4. **Performance Optimized**
   - Efficient query building with `OrderBy` and `ThenBy`
   - Deferred execution with IQueryable
   - Maximum sort fields limit (5) to prevent performance degradation

5. **Robust Validation**
   - Field name validation against allowed list
   - Sort direction validation (asc/desc only)
   - Normalization of input (trim, lowercase)
   - Comprehensive error reporting

## Usage Examples

### 1. Single Field Sort

**Query:**
```http
GET /api/v1/companies?sort=Name:asc
```

**Result:** Companies sorted by Name in ascending order

---

### 2. Multiple Fields Sort

**Query:**
```http
GET /api/v1/companies?sort=Country:asc&sort=Name:desc
```

**Result:** Companies sorted by:
1. Country (ascending)
2. Then by Name (descending) for companies in the same country

---

### 3. Default Sort Direction

**Query:**
```http
GET /api/v1/companies?sort=CreatedAt
```

**Result:** Companies sorted by CreatedAt in ascending order (default)

---

### 4. Complex Sorting with Filters

**Query:**
```http
GET /api/v1/companies?searchTerm=tech&currency=USD&isActive=true&sort=Industry:asc&sort=CreatedAt:desc&sort=Name:asc&page=1&pageSize=20
```

**Result:** Active USD tech companies sorted by:
1. Industry (ascending)
2. Then by CreatedAt (descending) within same industry
3. Then by Name (ascending) for same industry and date

---

### 5. Default Behavior (No Sort Specified)

**Query:**
```http
GET /api/v1/companies?searchTerm=software
```

**Result:** Companies sorted by Name in ascending order (default behavior)

---

## Valid Sort Fields

| Field Name | Data Type | Description |
|------------|-----------|-------------|
| `Name` | string | Company name |
| `Code` | string | Company unique code |
| `CreatedAt` | DateTime | Creation timestamp |
| `UpdatedAt` | DateTime | Last update timestamp |
| `Currency` | string | Currency code (USD, EUR, etc.) |
| `Country` | string | Company country |
| `Industry` | string | Industry sector |
| `City` | string | Company city |
| `IsActive` | boolean | Active status |
| `Email` | string | Company email |
| `SubscriptionPlan` | string | Subscription tier |

## Technical Implementation

### Architecture Overview

```
Controller (API Layer)
    ↓
Service (Application Layer) - Validation & Normalization
    ↓
Repository (Infrastructure Layer) - Query Building
    ↓
Database (PostgreSQL) - Efficient SQL Generation
```

### 1. DTO Layer - SortSpecification Record

```csharp
public record SortSpecification
{
    public required string Field { get; init; }
    public string Direction { get; init; } = "asc";
    
    // Parse from string: "Name:desc"
    public static SortSpecification? Parse(string sortString)
    {
        var parts = sortString.Split(':');
        return new SortSpecification
        {
            Field = parts[0],
            Direction = parts.Length > 1 ? parts[1] : "asc"
        };
    }
}
```

**Benefits:**
- Immutable (thread-safe)
- Easy to create and parse
- Clear validation methods

### 2. CompanySearchDto

```csharp
public class CompanySearchDto
{
    // Query string array: ?sort=Name:asc&sort=CreatedAt:desc
    public List<string>? Sort { get; set; }
    
    // Internal: Parsed specifications (populated during normalization)
    internal List<SortSpecification> SortBy { get; set; }
    
    // Maximum 5 fields for performance
    public const int MaxSortFields = 5;
    
    public ValidationResult Normalize()
    {
        // Parse Sort strings into SortBy list
        // Validate field names and directions
        // Remove duplicates
        // Limit to MaxSortFields
    }
}
```

**Features:**
- Query string format only (simple and consistent)
- Automatic normalization and validation
- Performance limits enforced

### 3. Repository Layer - Dynamic Sorting

```csharp
private IQueryable<Company> ApplyMultipleSort(
    IQueryable<Company> query,
    List<(string Field, string Direction)> sortSpecifications)
{
    IOrderedQueryable<Company>? orderedQuery = null;

    foreach (var (field, direction) in sortSpecifications)
    {
        var keySelector = GetSortKeySelector(field);

        if (orderedQuery == null)
        {
            // First sort: OrderBy/OrderByDescending
            orderedQuery = direction == "desc"
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }
        else
        {
            // Subsequent sorts: ThenBy/ThenByDescending
            orderedQuery = direction == "desc"
                ? orderedQuery.ThenByDescending(keySelector)
                : orderedQuery.ThenBy(keySelector);
        }
    }

    return orderedQuery ?? query;
}
```

**Benefits:**
- Dynamic field selection
- Proper use of `ThenBy` for multi-level sorting
- Single LINQ expression (efficient SQL generation)

### 4. SQL Generation Example

**Input:**
```http
?sort=Country:asc&sort=Name:desc
```

**Generated SQL:**
```sql
SELECT * FROM companies
WHERE ... -- filters
ORDER BY country ASC, name DESC
LIMIT 20 OFFSET 0
```

**Performance:** Single database query with proper ORDER BY clause

## Best Practices Applied

### 1. **Immutability for Thread-Safety**
```csharp
public record SortSpecification  // record = immutable by default
{
    public required string Field { get; init; }  // init-only setter
}
```

### 2. **Validation at Multiple Layers**
```csharp
// DTO validation
searchDto.Normalize();  // Returns ValidationResult
searchDto.ValidateSortFields();  // Field name validation

// Service layer validation
if (!validationResult.IsValid)
    return Error.Validation(...);
```

### 3. **Performance Limits**
```csharp
public const int MaxSortFields = 5;

if (SortBy.Count > MaxSortFields)
{
    errors.Add($"Maximum {MaxSortFields} sort fields allowed");
    SortBy = SortBy.Take(MaxSortFields).ToList();
}
```

### 4. **Clear Error Messages**
```csharp
errors.Add($"Invalid sort field '{sort.Field}'. " +
           $"Valid fields are: {string.Join(", ", validFields)}");
```

### 5. **Deferred Query Execution**
```csharp
var query = DbSet.AsQueryable();  // No database hit
query = ApplyFilters(query);      // Build expression tree
query = ApplySort(query);         // Build ORDER BY
var results = await query.ToListAsync();  // Single DB call
```

### 6. **Normalization and Sanitization**
```csharp
public SortSpecification Normalize()
{
    return this with
    {
        Field = Field.Trim(),
        Direction = Direction.ToLowerInvariant() == "desc" ? "desc" : "asc"
    };
}
```

### 7. **Duplicate Removal**
```csharp
SortBy = SortBy
    .GroupBy(s => s.Field.ToLowerInvariant())
    .Select(g => g.First())  // Keep first occurrence
    .ToList();
```

## Error Handling

### Invalid Field Name
**Request:**
```http
GET /api/v1/companies?sort=InvalidField:asc
```

**Response:**
```json
{
  "type": "ValidationError",
  "errors": [
    "Invalid sort field 'InvalidField'. Valid fields are: name, code, createdat, updatedat, currency, country, industry, city, isactive, email, subscriptionplan"
  ],
  "correlationId": "abc123..."
}
```

### Too Many Sort Fields
**Request:**
```http
GET /api/v1/companies?sort=Name&sort=Code&sort=Country&sort=City&sort=Industry&sort=Currency
```

**Response:**
```json
{
  "type": "ValidationError",
  "errors": [
    "Too many sort fields. Maximum 5 allowed, but 6 provided."
  ],
  "correlationId": "abc123..."
}
```

### Invalid Sort Direction
**Request:**
```http
GET /api/v1/companies?sort=Name:ascending
```

**Response:**
```json
{
  "type": "ValidationError",
  "errors": [
    "Invalid sort direction 'ascending'. Must be 'asc' or 'desc'"
  ],
  "correlationId": "abc123..."
}
```

## Performance Considerations

### Database Indexes

For optimal performance with sorting, ensure indexes exist on commonly sorted columns:

```sql
-- Single column indexes
CREATE INDEX idx_companies_name ON companies (name);
CREATE INDEX idx_companies_code ON companies (code);
CREATE INDEX idx_companies_created_at ON companies (created_at DESC);
CREATE INDEX idx_companies_currency ON companies (currency);
CREATE INDEX idx_companies_country ON companies (country);

-- Composite indexes for common sort combinations
CREATE INDEX idx_companies_country_name ON companies (country ASC, name ASC);
CREATE INDEX idx_companies_active_created ON companies (is_active, created_at DESC);
CREATE INDEX idx_companies_currency_country ON companies (currency, country);
```

### Query Performance Tips

1. **Limit sort fields**: Maximum 5 enforced
2. **Use indexes**: Ensure indexed columns for frequently sorted fields
3. **Pagination**: Always use with sorting for large datasets
4. **Monitor slow queries**: Use `EXPLAIN ANALYZE` in PostgreSQL

```sql
EXPLAIN ANALYZE
SELECT * FROM companies
WHERE is_active = true
ORDER BY country ASC, name DESC
LIMIT 20 OFFSET 0;
```

## Client-Side Integration

### JavaScript/TypeScript Example

```typescript
interface CompanySearchParams {
  searchTerm?: string;
  currency?: string;
  country?: string;
  page?: number;
  pageSize?: number;
  sort?: string[];  // Array of "field:direction" strings
}

async function searchCompanies(params: CompanySearchParams) {
  const queryParams = new URLSearchParams();
  
  // Add filters
  if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
  if (params.currency) queryParams.append('currency', params.currency);
  if (params.country) queryParams.append('country', params.country);
  
  // Add pagination
  queryParams.append('page', String(params.page || 1));
  queryParams.append('pageSize', String(params.pageSize || 20));
  
  // Add multiple sort fields
  params.sort?.forEach(sortSpec => {
    queryParams.append('sort', sortSpec);
  });
  
  const response = await fetch(
    `/api/v1/companies?${queryParams.toString()}`
  );
  
  return await response.json();
}

// Usage examples
await searchCompanies({
  searchTerm: 'tech',
  currency: 'USD',
  sort: ['Country:asc', 'Name:desc']
});

await searchCompanies({
  country: 'USA',
  isActive: true,
  page: 2,
  pageSize: 50,
  sort: ['Industry:asc', 'CreatedAt:desc', 'Name:asc']
});
```

### React Hook Example

```typescript
function useCompanySearch() {
  const [sort, setSort] = useState<string[]>(['Name:asc']);
  
  const toggleSort = (field: string) => {
    setSort(prevSort => {
      const existingIndex = prevSort.findIndex(s => s.startsWith(field));
      
      if (existingIndex >= 0) {
        const [, direction] = prevSort[existingIndex].split(':');
        const newDirection = direction === 'asc' ? 'desc' : 'asc';
        
        const newSort = [...prevSort];
        newSort[existingIndex] = `${field}:${newDirection}`;
        return newSort;
      } else {
        return [...prevSort, `${field}:asc`];
      }
    });
  };
  
  const removeSort = (field: string) => {
    setSort(prevSort => prevSort.filter(s => !s.startsWith(field)));
  };
  
  return { sort, toggleSort, removeSort };
}
```

## Testing

### Unit Test Examples

```csharp
[Fact]
public void SortSpecification_Parse_ValidString_ReturnsParsedObject()
{
    // Arrange
    var sortString = "Name:desc";
    
    // Act
    var result = SortSpecification.Parse(sortString);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Name", result.Field);
    Assert.Equal("desc", result.Direction);
}

[Fact]
public void CompanySearchDto_Normalize_TooManySortFields_LimitsToMax()
{
    // Arrange
    var dto = new CompanySearchDto
    {
        Sort = new List<string> { "Name", "Code", "Country", "City", "Industry", "Currency" }
    };
    
    // Act
    var result = dto.Normalize();
    
    // Assert
    Assert.False(result.IsValid);
    Assert.Contains("Maximum 5", result.GetErrorMessage());
    Assert.Equal(5, dto.SortBy.Count);
}

[Fact]
public async Task SearchAsync_MultipleSort_ReturnsCorrectOrder()
{
    // Arrange
    var searchDto = new CompanySearchDto
    {
        Sort = new List<string> { "Country:asc", "Name:desc" }
    };
    
    // Act
    var result = await _companyService.SearchAsync(searchDto);
    
    // Assert
    Assert.True(result.IsSuccess);
    var items = result.Value.Items.ToList();
    
    // Verify sorting order
    for (int i = 1; i < items.Count; i++)
    {
        var prev = items[i - 1];
        var curr = items[i];
        
        if (prev.Country == curr.Country)
        {
            Assert.True(string.Compare(prev.Name, curr.Name) >= 0);
        }
        else
        {
            Assert.True(string.Compare(prev.Country, curr.Country) <= 0);
        }
    }
}
```

## Migration Notes

### Removed Features

❌ **Legacy format no longer supported:**
- `sortField=Name&sortDirection=asc` (old format)

✅ **Use instead:**
- `sort=Name:asc` (new format)

### Migration Script for Clients

```typescript
// Old format
const oldParams = {
  sortField: 'Name',
  sortDirection: 'asc'
};

// Convert to new format
const newParams = {
  sort: [`${oldParams.sortField}:${oldParams.sortDirection}`]
};
```

## Summary

The multi-field sorting implementation follows industry best practices:

✅ **Type-safe** with immutable records
✅ **Flexible** input formats
✅ **Validated** at multiple layers
✅ **Performant** with proper limits and indexing
✅ **Well-documented** with comprehensive examples
✅ **Testable** with clear separation of concerns
✅ **Maintainable** with clear error messages
✅ **Scalable** with performance optimizations

The implementation provides a robust, production-ready sorting solution that handles edge cases, provides clear feedback, and performs efficiently with large datasets.

