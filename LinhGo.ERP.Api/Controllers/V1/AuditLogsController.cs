using Asp.Versioning;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.DTOs.Audit;
using LinhGo.SharedKernel.Api.Controllers;
using LinhGo.SharedKernel.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

/// <summary>
/// Audit log endpoints for viewing audit trail
/// Read-only access to audit logs
/// </summary>
[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/audit")]
[Tags("Audit")]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _auditLogService.GetByIdAsync(id);
        return ToResponse(result);
    }

    /// <summary>
    /// Get all audit logs with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _auditLogService.GetPagedAsync(page, pageSize);
        return ToResponse(result);
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    [HttpGet("entity/{entityName}/{entityId}")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityName, 
        string entityId,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        var result = await _auditLogService.GetByEntityAsync(entityName, entityId, page, pageSize);
        return ToResponse(result);
    }

    /// <summary>
    /// Get audit logs for a specific company
    /// </summary>
    [HttpGet("company/{companyId:guid}")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCompany(
        Guid companyId,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        var result = await _auditLogService.GetByCompanyAsync(companyId, page, pageSize);
        return ToResponse(result);
    }

    /// <summary>
    /// Get audit logs by user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(
        string userId,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        var result = await _auditLogService.GetByUserAsync(userId, page, pageSize);
        return ToResponse(result);
    }

    /// <summary>
    /// Query audit logs with filters
    /// </summary>
    [HttpPost("query")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Query([FromBody] AuditLogQueryDto query)
    {
        var result = await _auditLogService.QueryAsync(query);
        return ToResponse(result);
    }
}

