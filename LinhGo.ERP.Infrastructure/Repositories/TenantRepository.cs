using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class TenantRepository<T> : ITenantRepository<T> where T : TenantEntity
{
    protected readonly ErpDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public TenantRepository(ErpDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            e => e.CompanyId == companyId && e.Id == id,
            cancellationToken
        );
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CompanyId == companyId).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.CompanyId == companyId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(Guid companyId, T entity, CancellationToken cancellationToken = default)
    {
        entity.CompanyId = companyId;
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(Guid companyId, T entity, CancellationToken cancellationToken = default)
    {
        if (entity.CompanyId != companyId)
            throw new UnauthorizedAccessException("Cannot update entity from different company");

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(companyId, id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.CompanyId == companyId && e.Id == id, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(e => e.CompanyId == companyId, cancellationToken);
    }
}

