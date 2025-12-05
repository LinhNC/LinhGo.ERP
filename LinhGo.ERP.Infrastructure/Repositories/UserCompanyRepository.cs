using Microsoft.EntityFrameworkCore;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.ERP.Infrastructure.Data;

namespace LinhGo.ERP.Infrastructure.Repositories;

public class UserCompanyRepository : GenericRepository<UserCompany>, IUserCompanyRepository
{
    public UserCompanyRepository(ErpDbContext context) : base(context) { }

    public async Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.Company)
            .Include(uc => uc.Permissions)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.User)
            .Where(uc => uc.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId && uc.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserCompany>> GetActiveByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.User)
            .Where(uc => uc.CompanyId == companyId && uc.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserAssignedToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId && uc.IsActive, cancellationToken);
    }

    public async Task<UserCompany?> GetWithPermissionsAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uc => uc.Company)
            .Include(uc => uc.Permissions)
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);
    }

    public async Task AssignUserToCompanyAsync(Guid userId, Guid companyId, string role, CancellationToken cancellationToken = default)
    {
        var existing = await GetByUserAndCompanyAsync(userId, companyId, cancellationToken);
        
        if (existing != null)
        {
            // User already assigned, just update role and activate
            existing.Role = role;
            existing.IsActive = true;
            _dbSet.Update(existing);
        }
        else
        {
            // Create new assignment
            var userCompany = new UserCompany
            {
                UserId = userId,
                CompanyId = companyId,
                Role = role,
                IsActive = true
            };
            await _dbSet.AddAsync(userCompany, cancellationToken);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserFromCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        var userCompany = await GetByUserAndCompanyAsync(userId, companyId, cancellationToken);
        
        if (userCompany != null)
        {
            // Soft delete by marking as inactive
            userCompany.IsActive = false;
            _dbSet.Update(userCompany);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateRoleAsync(Guid userId, Guid companyId, string role, CancellationToken cancellationToken = default)
    {
        var userCompany = await GetByUserAndCompanyAsync(userId, companyId, cancellationToken);
        
        if (userCompany != null)
        {
            userCompany.Role = role;
            _dbSet.Update(userCompany);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

