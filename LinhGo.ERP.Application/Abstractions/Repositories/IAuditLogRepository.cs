using LinhGo.ERP.Domain.Audit.Entities;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for AuditLog operations
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Add a new audit log entry
    /// </summary>
    Task<AuditLog> AddAsync(AuditLog auditLog);
    
    /// <summary>
    /// Get audit log by ID
    /// </summary>
    Task<AuditLog?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get all audit logs with pagination
    /// </summary>
    Task<PagedResult<AuditLog>> GetPagedAsync(int page, int pageSize);
    
    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    Task<PagedResult<AuditLog>> GetByEntityAsync(string entityName, string entityId, int page, int pageSize);
    
    /// <summary>
    /// Get audit logs for a specific company
    /// </summary>
    Task<PagedResult<AuditLog>> GetByCompanyAsync(Guid companyId, int page, int pageSize);
    
    /// <summary>
    /// Get audit logs by user
    /// </summary>
    Task<PagedResult<AuditLog>> GetByUserAsync(string userId, int page, int pageSize);
    
    /// <summary>
    /// Get audit logs within a date range
    /// </summary>
    Task<PagedResult<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, int page, int pageSize);
    
    /// <summary>
    /// Get audit logs by action type (Create, Update, Delete)
    /// </summary>
    Task<PagedResult<AuditLog>> GetByActionAsync(string action, int page, int pageSize);
}

