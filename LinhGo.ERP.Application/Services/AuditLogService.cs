using System.Text.Json;
using AutoMapper;
using LinhGo.ERP.Application.Abstractions.Repositories;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Audit;
using LinhGo.SharedKernel.Result;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Services;

/// <summary>
/// Service for managing audit logs
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        IMapper mapper,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuditLogDetailDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var auditLog = await _auditLogRepository.GetByIdAsync(id);

            if (auditLog == null)
            {
                _logger.LogWarning("AuditLog with ID {AuditLogId} not found", id);
                return Error.WithNotFoundCode(AuditLogErrors.NotFound, id);
            }

            var result = _mapper.Map<AuditLogDetailDto>(auditLog);
            
            // Parse JSON values to objects
            if (!string.IsNullOrEmpty(auditLog.OldValues))
            {
                result.OldValuesObject = JsonSerializer.Deserialize<Dictionary<string, object?>>(auditLog.OldValues);
            }
            
            if (!string.IsNullOrEmpty(auditLog.NewValues))
            {
                result.NewValuesObject = JsonSerializer.Deserialize<Dictionary<string, object?>>(auditLog.NewValues);
            }
            
            // Build property changes
            result.PropertyChanges = BuildPropertyChanges(result.OldValuesObject, result.NewValuesObject);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log with ID {AuditLogId}", id);
            return Error.WithFailureCode(AuditLogErrors.GetByIdFailed);
        }
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            var pagedResult = await _auditLogRepository.GetPagedAsync(page, pageSize);
            
            var result = new PagedResult<AuditLogDto>
            {
                Items = _mapper.Map<List<AuditLogDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged audit logs");
            return Error.WithFailureCode(AuditLogErrors.GetPagedFailed);
        }
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetByEntityAsync(string entityName, string entityId, int page, int pageSize)
    {
        try
        {
            var pagedResult = await _auditLogRepository.GetByEntityAsync(entityName, entityId, page, pageSize);
            
            var result = new PagedResult<AuditLogDto>
            {
                Items = _mapper.Map<List<AuditLogDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity {EntityName}:{EntityId}", entityName, entityId);
            return Error.WithFailureCode(AuditLogErrors.GetByEntityFailed);
        }
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetByCompanyAsync(Guid companyId, int page, int pageSize)
    {
        try
        {
            var pagedResult = await _auditLogRepository.GetByCompanyAsync(companyId, page, pageSize);
            
            var result = new PagedResult<AuditLogDto>
            {
                Items = _mapper.Map<List<AuditLogDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for company {CompanyId}", companyId);
            return Error.WithFailureCode(AuditLogErrors.GetByCompanyFailed);
        }
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetByUserAsync(string userId, int page, int pageSize)
    {
        try
        {
            var pagedResult = await _auditLogRepository.GetByUserAsync(userId, page, pageSize);
            
            var result = new PagedResult<AuditLogDto>
            {
                Items = _mapper.Map<List<AuditLogDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return Error.WithFailureCode(AuditLogErrors.GetByUserFailed);
        }
    }

    public async Task<Result<PagedResult<AuditLogDto>>> QueryAsync(AuditLogQueryDto query)
    {
        try
        {
            // Validate date range
            if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate > query.ToDate)
            {
                return Error.WithValidationCode(AuditLogErrors.InvalidDateRange);
            }

            PagedResult<Domain.Audit.Entities.AuditLog> pagedResult;

            // Apply filters based on query
            if (!string.IsNullOrEmpty(query.EntityName) && !string.IsNullOrEmpty(query.EntityId))
            {
                pagedResult = await _auditLogRepository.GetByEntityAsync(
                    query.EntityName, query.EntityId, query.PageNumber, query.PageSize);
            }
            else if (query.CompanyId.HasValue)
            {
                pagedResult = await _auditLogRepository.GetByCompanyAsync(
                    query.CompanyId.Value, query.PageNumber, query.PageSize);
            }
            else if (!string.IsNullOrEmpty(query.UserId))
            {
                pagedResult = await _auditLogRepository.GetByUserAsync(
                    query.UserId, query.PageNumber, query.PageSize);
            }
            else if (query.FromDate.HasValue && query.ToDate.HasValue)
            {
                pagedResult = await _auditLogRepository.GetByDateRangeAsync(
                    query.FromDate.Value, query.ToDate.Value, query.PageNumber, query.PageSize);
            }
            else if (!string.IsNullOrEmpty(query.Action))
            {
                pagedResult = await _auditLogRepository.GetByActionAsync(
                    query.Action, query.PageNumber, query.PageSize);
            }
            else
            {
                pagedResult = await _auditLogRepository.GetPagedAsync(
                    query.PageNumber, query.PageSize);
            }

            var result = new PagedResult<AuditLogDto>
            {
                Items = _mapper.Map<List<AuditLogDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying audit logs");
            return Error.WithFailureCode(AuditLogErrors.QueryFailed);
        }
    }

    #region Helper Methods

    private List<PropertyChangeDto> BuildPropertyChanges(
        Dictionary<string, object?>? oldValues, 
        Dictionary<string, object?>? newValues)
    {
        var changes = new List<PropertyChangeDto>();

        if (newValues == null)
            return changes;

        foreach (var (key, newValue) in newValues)
        {
            var oldValue = oldValues?.GetValueOrDefault(key);
            
            // Only include if value actually changed
            if (!Equals(oldValue, newValue))
            {
                changes.Add(new PropertyChangeDto
                {
                    PropertyName = key,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }

        return changes;
    }

    #endregion
}

