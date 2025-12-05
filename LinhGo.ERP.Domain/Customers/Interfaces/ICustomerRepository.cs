using LinhGo.ERP.Domain.Common.Interfaces;
using LinhGo.ERP.Domain.Customers.Entities;

namespace LinhGo.ERP.Domain.Customers.Interfaces;

public interface ICustomerRepository : ITenantRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> SearchCustomersAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Customer?> GetWithDetailsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
}

