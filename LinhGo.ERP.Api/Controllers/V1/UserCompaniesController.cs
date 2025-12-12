using Asp.Versioning;
using LinhGo.ERP.Api.Models;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.ERP.Authorization.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

/// <summary>
/// User-Company relationship management endpoints
/// Manages user assignments to companies with roles and permissions
/// Multi-tenant: Requires user to have access to the company
/// </summary>
[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/users")]
[Tags("User Companies")]
[Authorize] // Require authentication for all endpoints
[RequireCompanyAccess] // Require user to have access to the company in route
public class UserCompaniesController(IUserCompanyService userCompanyService) : BaseApiController
{
    /// <summary>
    /// Get user-company relationship by ID
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="id">User-company relationship unique identifier</param>
    [HttpGet("{id:guid}")]
    [EndpointDescription("Get user-company relationship by ID")]
    [ProducesResponseType(typeof(UserCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid companyId, Guid id)
    {
        var result = await userCompanyService.GetByIdAsync(id);
        return ToResponse(result);
    }

    /// <summary>
    /// Get all users for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <remarks>
    /// Returns all users (active and inactive) assigned to the company.
    /// Includes user details and role information.
    /// </remarks>
    [HttpGet]
    [EndpointDescription("Get all users assigned to a specific company")]
    [ProducesResponseType(typeof(IEnumerable<UserCompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompanyUsers(Guid companyId)
    {
        var result = await userCompanyService.GetByCompanyIdAsync(companyId);
        return ToResponse(result);
    }
    

    /// <summary>
    /// Assign a user to a company with a specific role
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="dto">User-company assignment data</param>
    /// <remarks>
    /// Assigns a user to a company with the specified role.
    /// 
    /// Validations:
    /// - User must exist
    /// - Company must exist
    /// - User cannot be assigned to the same company twice
    /// 
    /// Common roles: Admin, Manager, Employee, Viewer
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(UserCompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignUserToCompany(Guid companyId, [FromBody] AssignUserToCompanyDto dto)
    {
        var result = await userCompanyService.AssignUserToCompanyAsync(dto);
        return ToCreatedResponse(result, nameof(GetById), new { companyId, id = result.IsError ? Guid.Empty : result.Value.Id });
    }

    /// <summary>
    /// Update user-company relationship (change role, default company, etc.)
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="id">User-company relationship ID</param>
    /// <param name="dto">Updated relationship data</param>
    /// <remarks>
    /// Updates the relationship between user and company.
    /// Can be used to change role, set default company, or activate/deactivate assignment.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid companyId, Guid id, [FromBody] AssignUserToCompanyDto dto)
    {
        var result = await userCompanyService.UpdateAsync(id, dto);
        return ToResponse(result);
    }

    /// <summary>
    /// Remove user from company (soft delete)
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="id">User-company relationship ID</param>
    /// <remarks>
    /// Removes the user's assignment from the company.
    /// This is a soft delete - the relationship is marked as inactive but retained for audit purposes.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveUserFromCompany(Guid companyId, Guid id)
    {
        var result = await userCompanyService.RemoveUserFromCompanyAsync(id);
        return ToNoContentResponse(result);
    }
}

