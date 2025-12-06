using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Inventory.Enums;
using LinhGo.ERP.Domain.Inventory.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class InventoryTransactionRepository(ErpDbContext context)
    : TenantRepository<InventoryTransaction>(context), IInventoryTransactionRepository
{
    public async Task<InventoryTransaction?> GetByTransactionNumberAsync(Guid companyId, string transactionNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(it => it.Product)
            .Include(it => it.FromWarehouse)
            .Include(it => it.ToWarehouse)
            .FirstOrDefaultAsync(it => it.CompanyId == companyId && it.TransactionNumber == transactionNumber, cancellationToken);
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(it => it.FromWarehouse)
            .Include(it => it.ToWarehouse)
            .Where(it => it.CompanyId == companyId && it.ProductId == productId)
            .OrderByDescending(it => it.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(it => it.Product)
            .Where(it => it.CompanyId == companyId && 
                        (it.FromWarehouseId == warehouseId || it.ToWarehouseId == warehouseId))
            .OrderByDescending(it => it.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(Guid companyId, TransactionType type, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(it => it.Product)
            .Include(it => it.FromWarehouse)
            .Include(it => it.ToWarehouse)
            .Where(it => it.CompanyId == companyId && it.Type == type)
            .OrderByDescending(it => it.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(Guid companyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(it => it.Product)
            .Include(it => it.FromWarehouse)
            .Include(it => it.ToWarehouse)
            .Where(it => it.CompanyId == companyId && 
                        it.TransactionDate >= startDate && 
                        it.TransactionDate <= endDate)
            .OrderByDescending(it => it.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByReferenceAsync(Guid companyId, string referenceType, Guid referenceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(it => it.Product)
            .Include(it => it.FromWarehouse)
            .Include(it => it.ToWarehouse)
            .Where(it => it.CompanyId == companyId && 
                        it.ReferenceType == referenceType && 
                        it.ReferenceId == referenceId)
            .OrderByDescending(it => it.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GenerateTransactionNumberAsync(Guid companyId, TransactionType type, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var typePrefix = type switch
        {
            TransactionType.StockIn => "IN",
            TransactionType.StockOut => "OUT",
            TransactionType.Adjustment => "ADJ",
            TransactionType.Transfer => "TRF",
            TransactionType.Return => "RET",
            TransactionType.Damaged => "DMG",
            TransactionType.Expired => "EXP",
            _ => "TXN"
        };
        
        var prefix = $"{typePrefix}-{year}-";
        
        var lastTransaction = await DbSet
            .Where(it => it.CompanyId == companyId && it.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(it => it.TransactionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastTransaction == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastTransaction.TransactionNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }
}

