using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Domain.Inventory.Interfaces;

public interface IStockRepository : ITenantRepository<Stock>
{
    Task<Stock?> GetByProductAndWarehouseAsync(Guid companyId, Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetByProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetByWarehouseAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetLowStockProductsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task UpdateStockLevelAsync(Guid companyId, Guid productId, Guid warehouseId, decimal quantityChange, CancellationToken cancellationToken = default);
}

