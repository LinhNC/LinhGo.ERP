using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Domain.Inventory.Interfaces;

public interface IProductRepository : ITenantRepository<Product>
{
    Task<Product?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(Guid companyId, string barcode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid companyId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchProductsAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Product?> GetWithStockAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
}

