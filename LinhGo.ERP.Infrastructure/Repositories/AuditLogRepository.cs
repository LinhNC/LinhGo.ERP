using LinhGo.ERP.Application.Abstractions.Repositories;
using LinhGo.ERP.Domain.Audit.Entities;
using LinhGo.ERP.Infrastructure.Data;
using LinhGo.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace LinhGo.ERP.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AuditLog operations
/// Provides read-only access to audit trail data
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly ErpDbContext _context;

    public AuditLogRepository(ErpDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByEntityAsync(string entityName, string entityId, int page, int pageSize)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByCompanyAsync(Guid companyId, int page, int pageSize)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByUserAsync(string userId, int page, int pageSize)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, int page, int pageSize)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.Timestamp >= from && a.Timestamp <= to)
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByActionAsync(string action, int page, int pageSize)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

