using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Inventory.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class WarehouseRepository : TenantRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ErpDbContext context) : base(context) { }

    public async Task<Warehouse?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.CompanyId == companyId && w.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.CompanyId == companyId && w.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Warehouse?> GetDefaultWarehouseAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.CompanyId == companyId && w.IsDefault && w.IsActive, cancellationToken);
    }

    public async Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(w => w.CompanyId == companyId && w.Code == code);

        if (excludeId.HasValue)
            query = query.Where(w => w.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }
}

