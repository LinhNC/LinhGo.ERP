using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class GenericRepository<T>(ErpDbContext context) : IRepository<T>
    where T : BaseEntity
{
    protected readonly ErpDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Get the entry for the entity
        var entry = Context.Entry(entity);
        
        // Store the version sent by the client (for concurrency check)
        var clientVersion = entity.Version;
        
        if (entry.State == EntityState.Detached)
        {
            // Entity is not tracked, we need to attach it
            DbSet.Attach(entity);
            entry.State = EntityState.Modified;
            
            // Set the original version to what the client sent
            // EF Core will generate: UPDATE ... WHERE Version = {clientVersion}
            entry.Property(nameof(BaseEntity.Version)).OriginalValue = clientVersion;
        }
        else
        {
            // Entity is already tracked (fetched earlier in the service)
            // Set the original version to what the client sent for concurrency check
            entry.Property(nameof(BaseEntity.Version)).OriginalValue = clientVersion;
        }
        
        // Save changes - will throw DbUpdateConcurrencyException if versions don't match
        await Context.SaveChangesAsync(cancellationToken);
        
        // Reload the entity to get the updated Version (xmin in PostgreSQL)
        await entry.ReloadAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
}

