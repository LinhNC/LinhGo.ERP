using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Inventory.Entities;
namespace LinhGo.ERP.Domain.Inventory.Interfaces;
public interface IWarehouseRepository : ITenantRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetDefaultWarehouseAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
