using Asp.Versioning;
using LinhGo.ERP.Api.Models;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.DTOs.Companies;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/companies")]
public class CompaniesController(ICompanyService companyService) : BaseApiController
{
    /// <summary>
    /// Search and filter companies with advanced query capabilities
    /// </summary>
    /// <remarks>
    /// Powerful search API with support for:
    /// - Full-text search across multiple fields
    /// - Dynamic field-based filtering with multiple operators
    /// - Multi-field sorting with direction control
    /// - Pagination
    /// - Related entity inclusion
    /// - Field selection
    /// 
    /// ## Query Parameters
    /// 
    /// ### 1. Full-Text Search (`q`)
    /// General search across name, code, email, etc.
    /// ```
    /// ?q=tech
    /// ```
    /// 
    /// ### 2. Dynamic Filtering (`filter[fieldName]` or `filter[fieldName][operator]`)
    /// 
    /// **Simple Equality (default `eq` operator):**
    /// ```
    /// ?filter[currency]=USD
    /// ?filter[country]=Vietnam
    /// ?filter[isActive]=true
    /// ```
    /// 
    /// **With Operators:**
    /// ```
    /// ?filter[name][contains]=Tech
    /// ?filter[currency][in]=USD,EUR,VND
    /// ?filter[city][startswith]=Ha
    /// ?filter[createdAt][gte]=2024-01-01
    /// ```
    /// 
    /// **Supported Operators:**
    /// - `eq` - Equals (default)
    /// - `neq`, `ne` - Not equals
    /// - `gt` - Greater than
    /// - `gte` - Greater than or equal
    /// - `lt` - Less than
    /// - `lte` - Less than or equal
    /// - `in` - In list (comma-separated)
    /// - `contains` - Contains substring
    /// - `startswith` - Starts with
    /// - `endswith` - Ends with
    /// 
    /// ### 3. Multi-Field Sorting (`sort`)
    /// 
    /// **Format:** `fieldName` for ASC, `-fieldName` for DESC
    /// ```
    /// ?sort=name                      # Name ascending
    /// ?sort=-createdAt                # CreatedAt descending
    /// ?sort=country,-name,createdAt   # Multi-level: Country ASC, Name DESC, CreatedAt ASC
    /// ```
    /// 
    /// ### 4. Pagination (`page`, `pageSize`)
    /// ```
    /// ?page=1&amp;pageSize=20
    /// ```
    /// 
    /// ### 5. Include Related Entities (`include`)
    /// ```
    /// ?include=settings
    /// ```
    /// 
    /// ### 6. Field Selection (`fields`)
    /// ```
    /// ?fields=id,name,code,currency
    /// ```
    /// 
    /// ## Complete Examples
    /// 
    /// **Example 1: Basic search with sort**
    /// ```
    /// GET /api/v1/companies?q=tech&amp;sort=name
    /// ```
    /// 
    /// **Example 2: Filter by multiple fields**
    /// ```
    /// GET /api/v1/companies?filter[currency]=USD&amp;filter[country]=Vietnam&amp;filter[isActive]=true
    /// ```
    /// 
    /// **Example 3: Complex query with operators**
    /// ```
    /// GET /api/v1/companies?filter[name][contains]=Tech&amp;filter[currency][in]=USD,EUR&amp;sort=-createdAt,name&amp;page=1&amp;pageSize=20
    /// ```
    /// 
    /// **Example 4: Search with field selection and includes**
    /// ```
    /// GET /api/v1/companies?q=software&amp;fields=id,name,code,currency&amp;include=settings&amp;sort=-createdAt
    /// ```
    /// 
    /// **Example 5: Date range filtering**
    /// ```
    /// GET /api/v1/companies?filter[createdAt][gte]=2024-01-01&amp;filter[createdAt][lte]=2024-12-31&amp;sort=-createdAt
    /// ```
    /// 
    /// ## Available Filter/Sort Fields for Companies
    /// `name`, `code`, `currency`, `country`, `industry`, `city`, `state`, `isActive`, `subscriptionPlan`, `email`, `phone`, `taxId`, `createdAt`, `updatedAt`
    /// 
    /// ## Response Format
    /// Returns a paginated result with:
    /// - `items`: Array of company objects
    /// - `totalCount`: Total number of matching records
    /// - `page`: Current page number
    /// - `pageSize`: Number of items per page
    /// </remarks>
    [Attributes.SearchableFields(
        entityName: "Companies",
        filterFields: ["name", "code", "currency", "country", "industry", "city", "state", "isActive", "subscriptionPlan", "email", "phone", "taxId", "createdAt", "updatedAt"
        ],
        sortFields: ["name", "code", "createdAt", "updatedAt", "currency", "country", "industry", "city", "isActive", "subscriptionPlan"
        ]
    )]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search([FromQuery] SearchQueryParams queries, CancellationToken ctx)
    {
        var result = await companyService.SearchAsync(queries, ctx);
        return ToResponse(result);
    }

    /// <summary>
    /// Get all companies
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await companyService.GetAllAsync();
        return ToResponse(result);
    }

    /// <summary>
    /// Get active companies only
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveCompanies()
    {
        var result = await companyService.GetActiveCompaniesAsync();
        return ToResponse(result);
    }

    /// <summary>
    /// Get company by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await companyService.GetByIdAsync(id);
        return ToResponse(result);
    }

    /// <summary>
    /// Get company by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await companyService.GetByCodeAsync(code);
        return ToResponse(result);
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        var result = await companyService.CreateAsync(dto);
        return ToCreatedResponse(result, nameof(GetById), new { id = result.IsError ? Guid.Empty : result.Value.Id });
    }

    /// <summary>
    /// Update an existing company
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        var result = await companyService.UpdateAsync(id, dto);
        return ToResponse(result);
    }

    /// <summary>
    /// Delete a company
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await companyService.DeleteAsync(id);
        return ToNoContentResponse(result);
    }
}

