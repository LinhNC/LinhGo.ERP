using Asp.Versioning;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Companies;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CompaniesController(ICompanyService companyService) : BaseApiController
{
    /// <summary>
    /// Get all companies
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new 
            { 
                Type = "Validation",
                Errors = new[] 
                { 
                    new 
                    { 
                        Code = GeneralErrors.ValidationFailed, 
                        Description = "ID mismatch between route and body",
                        Field = "id"
                    } 
                },
                CorrelationId = CorrelationIdService.GetCorrelationId()
            });
        }

        var result = await companyService.UpdateAsync(dto);
        return ToResponse(result);
    }

    /// <summary>
    /// Delete a company
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await companyService.DeleteAsync(id);
        return ToNoContentResponse(result);
    }
}

