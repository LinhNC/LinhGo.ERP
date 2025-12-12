using LinhGo.ERP.Application.DTOs.Audit;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Application.Abstractions.Services;

/// <summary>
/// Service interface for AuditLog operations
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Get audit log by ID
    /// </summary>
    Task<Result<AuditLogDetailDto>> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get all audit logs with pagination
    /// </summary>
    Task<Result<PagedResult<AuditLogDto>>> GetPagedAsync(int page, int pageSize);
    
    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    Task<Result<PagedResult<AuditLogDto>>> GetByEntityAsync(string entityName, string entityId, int page, int pageSize);
    
    /// <summary>
    /// Get audit logs for a specific company
    /// </summary>
    Task<Result<PagedResult<AuditLogDto>>> GetByCompanyAsync(Guid companyId, int page, int pageSize);
    
    /// <summary>
    /// Get audit logs by user
    /// </summary>
    Task<Result<PagedResult<AuditLogDto>>> GetByUserAsync(string userId, int page, int pageSize);
    
    /// <summary>
    /// Query audit logs with filters
    /// </summary>
    Task<Result<PagedResult<AuditLogDto>>> QueryAsync(AuditLogQueryDto query);
}

