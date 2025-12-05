using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Orders.Entities;
using LinhGo.ERP.Domain.Orders.Enums;

namespace LinhGo.ERP.Domain.Orders.Interfaces;

public interface IOrderRepository : ITenantRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(Guid companyId, string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(Guid companyId, OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByDateRangeAsync(Guid companyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<string> GenerateOrderNumberAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Order?> GetWithDetailsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
}

