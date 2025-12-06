using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Orders.Entities;
using LinhGo.ERP.Domain.Orders.Enums;
using LinhGo.ERP.Domain.Orders.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class OrderRepository(ErpDbContext context) : TenantRepository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(Guid companyId, string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(o => o.CompanyId == companyId && o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CompanyId == companyId && o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(Guid companyId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CompanyId == companyId && o.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(Guid companyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CompanyId == companyId && o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GenerateOrderNumberAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"ORD-{year}-";
        
        var lastOrder = await DbSet
            .Where(o => o.CompanyId == companyId && o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastOrder == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }

    public async Task<Order?> GetWithDetailsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Include(o => o.Shipments)
                .ThenInclude(s => s.Items)
            .FirstOrDefaultAsync(o => o.CompanyId == companyId && o.Id == id, cancellationToken);
    }
}

