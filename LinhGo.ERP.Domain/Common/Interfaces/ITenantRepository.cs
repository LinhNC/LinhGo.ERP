using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Common.Interfaces;

/// <summary>
/// Repository interface for tenant-specific entities
/// Automatically filters by CompanyId
/// </summary>
public interface ITenantRepository<T> where T : TenantEntity
{
    Task<T?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetPagedAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<T> AddAsync(Guid companyId, T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid companyId, T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}

