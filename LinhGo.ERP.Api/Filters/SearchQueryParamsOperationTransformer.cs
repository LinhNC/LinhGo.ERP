using LinhGo.ERP.Domain.Common;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace LinhGo.ERP.Api.Filters;

/// <summary>
/// OpenAPI operation transformer to replace SearchQueryParams with custom query parameter descriptions
/// </summary>
public class SearchQueryParamsOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Check if this operation has SearchQueryParams parameter
        var hasSearchQueryParams = context.Description.ParameterDescriptions
            .Any(p => p.ParameterDescriptor.ParameterType == typeof(SearchQueryParams));

        if (!hasSearchQueryParams)
            return Task.CompletedTask;

        // Get SearchableFieldsAttribute from the action method
        var searchableFieldsAttr = context.Description.ActionDescriptor.EndpointMetadata?
            .OfType<Attributes.SearchableFieldsAttribute>()
            .FirstOrDefault();

        var entityName = searchableFieldsAttr?.EntityName ?? "this entity";
        var filterFields = searchableFieldsAttr?.FilterFields ?? Array.Empty<string>();
        var sortFields = searchableFieldsAttr?.SortFields ?? Array.Empty<string>();

        // Remove the auto-generated SearchQueryParams parameters
        var parametersToRemove = operation.Parameters
            .Where(p => p.In == ParameterLocation.Query && 
                       (p.Name.StartsWith("Filters", StringComparison.OrdinalIgnoreCase) || 
                        p.Name.StartsWith("Fields", StringComparison.OrdinalIgnoreCase) || 
                        p.Name.Equals("Q", StringComparison.OrdinalIgnoreCase) || 
                        p.Name.Equals("Sorts", StringComparison.OrdinalIgnoreCase) || 
                        p.Name.Equals("Includes", StringComparison.OrdinalIgnoreCase) || 
                        p.Name.Equals("Page", StringComparison.OrdinalIgnoreCase) || 
                        p.Name.Equals("PageSize", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var param in parametersToRemove)
        {
            operation.Parameters.Remove(param);
        }

        // Add custom parameter descriptions with examples
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "q",
            In = ParameterLocation.Query,
            Description = "General search term (searches across name, code, email, etc.)",
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        });

        // Format filter fields with bold styling
        var filterFieldsText = filterFields.Length > 0 
            ? $"**Available filter fields for {entityName}:**\n\n" + 
              string.Join("\n", filterFields.Select(f => $"- **`{f}`**"))
            : $"**Note:** Filter fields are dynamically determined based on {entityName} properties.";

        // Generate examples using actual field names if available
        var filterExamples = filterFields.Length > 0
            ? GenerateFilterExamples(filterFields, entityName)
            : "**Examples:**\n" +
              "- `filter[fieldName]=value` - Simple equality\n" +
              "- `filter[fieldName][contains]=text` - Contains operator\n" +
              "- `filter[fieldName][in]=val1,val2` - In list operator";

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "filter[fieldName]",
            In = ParameterLocation.Query,
            Description = "**Dynamic field-based filtering** - Replace `[fieldName]` with actual field name from available fields below.\n\n" +
                         "### Format\n" +
                         "- **Simple:** `filter[fieldName]=value` (defaults to `eq` operator)\n" +
                         "- **With operator:** `filter[fieldName][operator]=value`\n\n" +
                         "### Supported Operators\n" +
                         "| Operator | Description | Example |\n" +
                         "|----------|-------------|----------|\n" +
                         "| `eq` | Equals (default) | `filter[currency]=USD` |\n" +
                         "| `neq`, `ne` | Not equals | `filter[status][neq]=inactive` |\n" +
                         "| `gt` | Greater than | `filter[price][gt]=100` |\n" +
                         "| `gte` | Greater than or equal | `filter[price][gte]=50` |\n" +
                         "| `lt` | Less than | `filter[stock][lt]=10` |\n" +
                         "| `lte` | Less than or equal | `filter[stock][lte]=5` |\n" +
                         "| `in` | In list (comma-separated) | `filter[currency][in]=USD,EUR,VND` |\n" +
                         "| `contains` | String contains | `filter[name][contains]=Tech` |\n" +
                         "| `startswith` | String starts with | `filter[code][startswith]=COM` |\n" +
                         "| `endswith` | String ends with | `filter[email][endswith]=@gmail.com` |\n\n" +
                         filterExamples + "\n\n" +
                         filterFieldsText,
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        });

        // Format sort fields with bold styling
        var sortFieldsText = sortFields.Length > 0 
            ? $"**Valid sort fields for {entityName}:**\n\n" + 
              string.Join("\n", sortFields.Select(f => $"- **`{f}`**"))
            : $"**Note:** Sort fields are dynamically determined based on {entityName} properties.";

        // Generate sort examples using actual field names if available
        var sortExamples = sortFields.Length > 0
            ? GenerateSortExamples(sortFields, entityName)
            : "**Examples:**\n" +
              "- `fieldName` - Sort ascending\n" +
              "- `-fieldName` - Sort descending\n" +
              "- `field1,-field2` - Multi-field sort";

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "sort",
            In = ParameterLocation.Query,
            Description = "**Multi-field sorting** with direction control.\n\n" +
                         "### Format\n" +
                         "- **Ascending:** `fieldName` (no prefix)\n" +
                         "- **Descending:** `-fieldName` (prefix with `-`)\n" +
                         "- **Multiple fields:** Comma-separated `field1,-field2,field3`\n\n" +
                         "### Sort Priority\n" +
                         "First field = primary sort, subsequent fields = secondary sort (ThenBy)\n\n" +
                         sortExamples + "\n\n" +
                         sortFieldsText,
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "page",
            In = ParameterLocation.Query,
            Description = "Page number for pagination (default: 1, min: 1)",
            Required = false,
            Schema = new OpenApiSchema 
            { 
                Type = JsonSchemaType.Integer, 
                Minimum = "1"
            },
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "pageSize",
            In = ParameterLocation.Query,
            Description = "Number of items per page (default: 20, min: 1, max: 500)",
            Required = false,
            Schema = new OpenApiSchema 
            { 
                Type = JsonSchemaType.Integer, 
                Minimum = "1",
                Maximum = "100", 
            },
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "include",
            In = ParameterLocation.Query,
            Description = "Related entities to include in response (comma-separated). Example: 'settings'",
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });

        // Format selectable fields with bold styling
        var selectFieldsText = filterFields.Length > 0 
            ? $"**Available fields for {entityName}:**\n\n" + 
              string.Join("\n", filterFields.Select(f => $"- **`{f}`**"))
            : $"**Note:** Selectable fields are dynamically determined based on {entityName} properties.";

        // Generate field selection examples using actual field names
        var fieldsExamples = filterFields.Length > 0
            ? GenerateFieldsExamples(filterFields, entityName)
            : "**Examples:**\n" +
              "- `fields=id,name,code` - Select specific fields\n" +
              "- `fields=id,name,field1,field2` - Select multiple fields";

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "fields",
            In = ParameterLocation.Query,
            Description = "**Field selection** - Return only specific fields in response (comma-separated).\n\n" +
                         "Use this to reduce payload size by selecting only needed fields.\n\n" +
                         "### Format\n" +
                         "`fields=field1,field2,field3`\n\n" +
                         fieldsExamples + "\n\n" +
                         selectFieldsText,
            Required = false,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Generate filter examples using actual field names from the entity
    /// </summary>
    private static string GenerateFilterExamples(string[] filterFields, string entityName)
    {
        var examples = new List<string>();
        
        // Try to find common field patterns and generate relevant examples
        var stringField = filterFields.FirstOrDefault(f => f.Equals("name", StringComparison.OrdinalIgnoreCase)) 
                         ?? filterFields.FirstOrDefault(f => f.Equals("code", StringComparison.OrdinalIgnoreCase))
                         ?? filterFields.FirstOrDefault(f => f.Contains("name", StringComparison.OrdinalIgnoreCase));
        
        var boolField = filterFields.FirstOrDefault(f => f.Equals("isActive", StringComparison.OrdinalIgnoreCase))
                       ?? filterFields.FirstOrDefault(f => f.StartsWith("is", StringComparison.OrdinalIgnoreCase));
        
        var enumField = filterFields.FirstOrDefault(f => f.Equals("currency", StringComparison.OrdinalIgnoreCase))
                       ?? filterFields.FirstOrDefault(f => f.Equals("country", StringComparison.OrdinalIgnoreCase))
                       ?? filterFields.FirstOrDefault(f => f.Equals("status", StringComparison.OrdinalIgnoreCase));
        
        var dateField = filterFields.FirstOrDefault(f => f.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
                       ?? filterFields.FirstOrDefault(f => f.Equals("updatedAt", StringComparison.OrdinalIgnoreCase))
                       ?? filterFields.FirstOrDefault(f => f.Contains("date", StringComparison.OrdinalIgnoreCase));

        examples.Add($"### Real Examples for {entityName}\n");

        // Simple equality examples
        if (enumField != null)
            examples.Add($"**Simple equality:**\n```\nfilter[{enumField}]=USD\n```");
        
        if (boolField != null)
            examples.Add($"**Boolean filter:**\n```\nfilter[{boolField}]=true\n```");

        // String operations
        if (stringField != null)
        {
            examples.Add($"**String contains:**\n```\nfilter[{stringField}][contains]=Tech\n```");
            examples.Add($"**String starts with:**\n```\nfilter[{stringField}][startswith]=COM\n```");
        }

        // In operator
        if (enumField != null)
            examples.Add($"**In list:**\n```\nfilter[{enumField}][in]=USD,EUR,VND\n```");

        // Date range
        if (dateField != null)
        {
            examples.Add($"**Date range:**\n```\nfilter[{dateField}][gte]=2024-01-01&filter[{dateField}][lte]=2024-12-31\n```");
        }

        // Multiple filters
        var multipleFilters = new List<string>();
        if (enumField != null) multipleFilters.Add($"filter[{enumField}]=USD");
        if (boolField != null) multipleFilters.Add($"filter[{boolField}]=true");
        if (stringField != null) multipleFilters.Add($"filter[{stringField}][contains]=Tech");
        
        if (multipleFilters.Count > 1)
            examples.Add($"**Multiple filters:**\n```\n{string.Join("&", multipleFilters)}\n```");

        return string.Join("\n\n", examples);
    }

    /// <summary>
    /// Generate sort examples using actual field names from the entity
    /// </summary>
    private static string GenerateSortExamples(string[] sortFields, string entityName)
    {
        var examples = new List<string>();
        
        var nameField = sortFields.FirstOrDefault(f => f.Equals("name", StringComparison.OrdinalIgnoreCase))
                       ?? sortFields.FirstOrDefault(f => f.Contains("name", StringComparison.OrdinalIgnoreCase))
                       ?? sortFields.FirstOrDefault();
        
        var dateField = sortFields.FirstOrDefault(f => f.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
                       ?? sortFields.FirstOrDefault(f => f.Equals("updatedAt", StringComparison.OrdinalIgnoreCase))
                       ?? sortFields.FirstOrDefault(f => f.Contains("date", StringComparison.OrdinalIgnoreCase));
        
        var secondaryField = sortFields.FirstOrDefault(f => f.Equals("code", StringComparison.OrdinalIgnoreCase))
                            ?? sortFields.FirstOrDefault(f => f.Equals("country", StringComparison.OrdinalIgnoreCase))
                            ?? sortFields.Skip(1).FirstOrDefault();

        examples.Add($"### Real Examples for {entityName}\n");

        if (nameField != null)
        {
            examples.Add($"**Sort ascending:**\n```\nsort={nameField}\n```");
        }

        if (dateField != null)
        {
            examples.Add($"**Sort descending:**\n```\nsort=-{dateField}\n```");
        }

        if (nameField != null && dateField != null)
        {
            examples.Add($"**Multi-field sort:**\n```\nsort={nameField},-{dateField}\n```");
        }

        if (secondaryField != null && nameField != null && dateField != null)
        {
            examples.Add($"**Three-level sort:**\n```\nsort={secondaryField},-{nameField},{dateField}\n```\n" +
                        $"(Sort by **`{secondaryField}`** ASC, then **`{nameField}`** DESC, then **`{dateField}`** ASC)");
        }

        return string.Join("\n\n", examples);
    }

    /// <summary>
    /// Generate field selection examples using actual field names from the entity
    /// </summary>
    private static string GenerateFieldsExamples(string[] filterFields, string entityName)
    {
        var examples = new List<string>();
        
        // Get some common fields for examples
        var idField = "id";
        var nameField = filterFields.FirstOrDefault(f => f.Equals("name", StringComparison.OrdinalIgnoreCase));
        var codeField = filterFields.FirstOrDefault(f => f.Equals("code", StringComparison.OrdinalIgnoreCase));
        var otherFields = filterFields.Where(f => 
            !f.Equals("name", StringComparison.OrdinalIgnoreCase) && 
            !f.Equals("code", StringComparison.OrdinalIgnoreCase) &&
            !f.Equals("id", StringComparison.OrdinalIgnoreCase))
            .Take(3)
            .ToArray();

        examples.Add($"### Real Examples for {entityName}\n");

        // Basic selection
        var basicFields = new List<string> { idField };
        if (nameField != null) basicFields.Add(nameField);
        if (codeField != null) basicFields.Add(codeField);
        
        if (basicFields.Count > 1)
        {
            examples.Add($"**Select basic fields:**\n```\nfields={string.Join(",", basicFields)}\n```");
        }

        // Extended selection
        if (otherFields.Length >= 2)
        {
            var extendedFields = basicFields.Concat(otherFields.Take(2)).ToList();
            examples.Add($"**Select multiple fields:**\n```\nfields={string.Join(",", extendedFields)}\n```");
        }

        // Minimal selection
        if (nameField != null)
        {
            examples.Add($"**Minimal response:**\n```\nfields={idField},{nameField}\n```");
        }

        return string.Join("\n\n", examples);
    }
}
