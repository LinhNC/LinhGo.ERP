using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Inventory.Enums;
namespace LinhGo.ERP.Domain.Inventory.Interfaces;
public interface IInventoryTransactionRepository : ITenantRepository<InventoryTransaction>
{
    Task<InventoryTransaction?> GetByTransactionNumberAsync(Guid companyId, string transactionNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(Guid companyId, TransactionType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(Guid companyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetByReferenceAsync(Guid companyId, string referenceType, Guid referenceId, CancellationToken cancellationToken = default);
    Task<string> GenerateTransactionNumberAsync(Guid companyId, TransactionType type, CancellationToken cancellationToken = default);
}
