using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Inventory.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class ProductRepository : TenantRepository<Product>, IProductRepository
{
    public ProductRepository(ErpDbContext context) : base(context) { }

    public async Task<Product?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Code == code, cancellationToken);
    }

    public async Task<Product?> GetByBarcodeAsync(Guid companyId, string barcode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Barcode == barcode, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid companyId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompanyId == companyId && p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompanyId == companyId &&
                (p.Name.Contains(searchTerm) ||
                 p.Code.Contains(searchTerm) ||
                 (p.Barcode != null && p.Barcode.Contains(searchTerm))))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.CompanyId == companyId && p.Code == code);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<Product?> GetWithStockAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Stocks)
                .ThenInclude(s => s.Warehouse)
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == id, cancellationToken);
    }
}

