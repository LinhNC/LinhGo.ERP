using Asp.Versioning;
using LinhGo.ERP.Api.Models;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.DTOs.Companies;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/companies")]
public class CompaniesController(ICompanyService companyService) : BaseApiController
{
    /// <summary>
    /// Search and filter companies with advanced query capabilities
    /// </summary>
    [QuerierFieldsAttribute(
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
    public async Task<IActionResult> Search([FromQuery] QuerierParams queries, CancellationToken ctx)
    {
        var result = await companyService.QueryAsync(queries, ctx);
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

