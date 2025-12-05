using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ErpDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.UserCompanies!.Any(uc => uc.CompanyId == companyId && uc.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.Email == email);

        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<User?> GetWithCompaniesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
}

