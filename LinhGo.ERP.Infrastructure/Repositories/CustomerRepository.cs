using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Customers.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class CustomerRepository : TenantRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(ErpDbContext context) : base(context) { }

    public async Task<Customer?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Contacts)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId &&
                (c.Name.Contains(searchTerm) ||
                 c.Code.Contains(searchTerm) ||
                 (c.Email != null && c.Email.Contains(searchTerm))))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCodeUniqueAsync(Guid companyId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.CompanyId == companyId && c.Code == code);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<Customer?> GetWithDetailsAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Contacts)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == id, cancellationToken);
    }
}

