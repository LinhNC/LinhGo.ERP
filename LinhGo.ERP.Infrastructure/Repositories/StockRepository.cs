using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Inventory.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class StockRepository(ErpDbContext context) : TenantRepository<Stock>(context), IStockRepository
{
    public async Task<Stock?> GetByProductAndWarehouseAsync(Guid companyId, Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.CompanyId == companyId && 
                                      s.ProductId == productId && 
                                      s.WarehouseId == warehouseId, 
                                 cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetByProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Warehouse)
            .Where(s => s.CompanyId == companyId && s.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetByWarehouseAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Product)
            .Where(s => s.CompanyId == companyId && s.WarehouseId == warehouseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetLowStockProductsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.CompanyId == companyId && 
                        s.Product != null && 
                        s.QuantityOnHand <= s.Product.ReorderLevel)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStockLevelAsync(Guid companyId, Guid productId, Guid warehouseId, decimal quantityChange, CancellationToken cancellationToken = default)
    {
        var stock = await GetByProductAndWarehouseAsync(companyId, productId, warehouseId, cancellationToken);
        
        if (stock == null)
        {
            stock = new Stock
            {
                CompanyId = companyId,
                ProductId = productId,
                WarehouseId = warehouseId,
                QuantityOnHand = quantityChange
            };
            await DbSet.AddAsync(stock, cancellationToken);
        }
        else
        {
            stock.QuantityOnHand += quantityChange;
            DbSet.Update(stock);
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}

