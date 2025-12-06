using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class TenantRepository<T>(ErpDbContext context) : ITenantRepository<T>
    where T : TenantEntity
{
    protected readonly ErpDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            e => e.CompanyId == companyId && e.Id == id,
            cancellationToken
        );
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.CompanyId == companyId).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.CompanyId == companyId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(Guid companyId, T entity, CancellationToken cancellationToken = default)
    {
        entity.CompanyId = companyId;
        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(Guid companyId, T entity, CancellationToken cancellationToken = default)
    {
        if (entity.CompanyId != companyId)
            throw new UnauthorizedAccessException("Cannot update entity from different company");

        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(companyId, id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.CompanyId == companyId && e.Id == id, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(e => e.CompanyId == companyId, cancellationToken);
    }
}

