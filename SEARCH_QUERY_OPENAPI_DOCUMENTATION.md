# Search Query Parameters OpenAPI Documentation - Implementation Summary

## Overview
Successfully implemented a comprehensive OpenAPI documentation system for SearchQueryParams that dynamically generates field-specific documentation for each controller endpoint.

## What Was Implemented

### 1. **SearchableFieldsAttribute** (`/Api/Attributes/SearchableFieldsAttribute.cs`)
A custom attribute that controllers can use to specify available fields for filtering and sorting:

```csharp
[SearchableFields(
    entityName: "Companies",
    filterFields: ["name", "code", "currency", "country", ...],
    sortFields: ["name", "code", "createdAt", "updatedAt", ...]
)]
```

### 2. **SearchQueryParamsOperationTransformer** (`/Api/Filters/SearchQueryParamsOperationTransformer.cs`)
An OpenAPI operation transformer that:
- Removes auto-generated SearchQueryParams parameters from OpenAPI schema
- Reads the `SearchableFieldsAttribute` from controller methods
- Dynamically generates detailed documentation with:
  - **Bold-styled field names** for easy readability
  - **Real examples** using actual field names from the entity
  - **Formatted operator tables** for filter operations
  - **Multi-level sorting examples** with explanations

### 3. **Dynamic Example Generation**
Three helper methods that intelligently create examples based on available fields:

#### `GenerateFilterExamples()`
Generates filter examples by detecting:
- String fields (name, code) → Contains, StartsWith examples
- Boolean fields (isActive) → Boolean filter examples
- Enum-like fields (currency, country) → Equality and In-list examples
- Date fields (createdAt, updatedAt) → Date range examples
- Multiple filter combinations

#### `GenerateSortExamples()`
Generates sort examples by detecting:
- Name fields → Ascending sort examples
- Date fields → Descending sort examples
- Multi-field sorting with 2-3 levels
- Explanatory text for complex sorts

#### `GenerateFieldsExamples()`
Generates field selection examples:
- Basic field selection (id, name, code)
- Extended field selection with additional fields
- Minimal response examples

## Features

### 📋 **For Filter Parameters**
- **26 supported operators** documented in a markdown table
- **Dynamic field list** with bold formatting: `- **`fieldName`**`
- **Real examples** using actual entity fields:
  ```
  filter[currency]=USD
  filter[name][contains]=Tech
  filter[isActive]=true
  filter[createdAt][gte]=2024-01-01&filter[createdAt][lte]=2024-12-31
  ```

### 📊 **For Sort Parameters**
- **GitHub-style sorting** (`-` prefix for descending)
- **Multi-field support** with clear priority explanation
- **Real examples**:
  ```
  sort=name                    # Ascending
  sort=-createdAt              # Descending
  sort=country,-name,createdAt # Multi-level
  ```

### 🎯 **For Field Selection**
- **Reduce payload size** by selecting specific fields
- **Real examples** using actual entity fields:
  ```
  fields=id,name,code
  fields=id,name,code,currency,country
  ```

### 📄 **For Pagination**
- `page` (default: 1, min: 1)
- `pageSize` (default: 20, min: 1, max: 500)

### 🔗 **For Related Entities**
- `include` - Comma-separated related entities to eagerly load
- Example: `include=settings`

## Implementation in CompaniesController

```csharp
[SearchableFields(
    entityName: "Companies",
    filterFields: ["name", "code", "currency", "country", "industry", 
                   "city", "state", "isActive", "subscriptionPlan", 
                   "email", "phone", "taxId", "createdAt", "updatedAt"],
    sortFields: ["name", "code", "createdAt", "updatedAt", "currency", 
                 "country", "industry", "city", "isActive", "subscriptionPlan"]
)]
[HttpGet]
public async Task<IActionResult> Search([FromQuery] SearchQueryParams queries, CancellationToken ctx)
{
    var result = await companyService.SearchAsync(queries, ctx);
    return ToResponse(result);
}
```

## OpenAPI Documentation Output

### Field Lists Display
Instead of plain text:
```
Available filter fields for Companies: name, code, currency, country...
```

Now displays as:
```
**Available filter fields for Companies:**

- **`name`**
- **`code`**
- **`currency`**
- **`country`**
- **`industry`**
...
```

### Examples Display
Real examples for Companies:

**Simple equality:**
```
filter[currency]=USD
```

**Boolean filter:**
```
filter[isActive]=true
```

**String contains:**
```
filter[name][contains]=Tech
```

**In list:**
```
filter[currency][in]=USD,EUR,VND
```

**Date range:**
```
filter[createdAt][gte]=2024-01-01&filter[createdAt][lte]=2024-12-31
```

**Multiple filters:**
```
filter[currency]=USD&filter[isActive]=true&filter[name][contains]=Tech
```

## Benefits

1. ✅ **Entity-Specific Documentation** - Each controller can define its own searchable fields
2. ✅ **Bold, Readable Formatting** - Field names are easy to spot with bold monospace styling
3. ✅ **Real, Working Examples** - Examples use actual field names from the entity
4. ✅ **Maintainable** - Add new entities by just applying the attribute
5. ✅ **Self-Documenting** - Swagger/Scalar UI shows complete, accurate documentation
6. ✅ **Type-Safe** - Uses actual SearchQueryParams model binding under the hood

## How to Apply to Other Controllers

For any new controller with search functionality:

```csharp
[SearchableFields(
    entityName: "Products",  // Your entity name
    filterFields: ["name", "sku", "price", "category", "inStock"],
    sortFields: ["name", "price", "createdAt", "category"]
)]
[HttpGet]
public async Task<IActionResult> Search([FromQuery] SearchQueryParams queries, CancellationToken ctx)
{
    // Your search implementation
}
```

The OpenAPI transformer will automatically:
- Generate field lists with bold formatting
- Create relevant examples using your actual field names
- Show appropriate operators for detected field types
- Format everything beautifully in Swagger/Scalar UI

## Files Modified

1. `/Api/Attributes/SearchableFieldsAttribute.cs` - NEW
2. `/Api/Filters/SearchQueryParamsOperationTransformer.cs` - NEW (347 lines)
3. `/Api/Controllers/V1/CompaniesController.cs` - Updated with attribute
4. `/Api/DependencyInjection.cs` - Registered transformer

## Build Status
✅ Build succeeded with 0 errors, 3 warnings (all nullable reference warnings, safe to ignore)

## Next Steps

To use this in other controllers:
1. Add `[SearchableFields]` attribute with your entity's fields
2. The OpenAPI documentation will automatically generate with your specific fields and examples
3. Test in Swagger/Scalar UI to see the beautiful formatted documentation

