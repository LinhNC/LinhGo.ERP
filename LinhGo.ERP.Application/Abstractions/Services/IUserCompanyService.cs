using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Application.Abstractions.Services;

/// <summary>
/// UserCompany service interface
/// Manages user-company relationships with roles and permissions
/// </summary>
public interface IUserCompanyService
{
    // Query operations
    Task<Result<UserCompanyDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<UserCompanyDto>>> GetByUserIdAsync(Guid userId);
    Task<Result<IEnumerable<UserCompanyDto>>> GetByCompanyIdAsync(Guid companyId);
    Task<Result<UserCompanyDto>> GetByUserAndCompanyAsync(Guid userId, Guid companyId);
    Task<Result<PagedResult<UserCompanyDto>>> QueryAsync(QuerierParams queries, CancellationToken ctx);
    
    // Command operations
    Task<Result<UserCompanyDto>> AssignUserToCompanyAsync(AssignUserToCompanyDto dto);
    Task<Result<UserCompanyDto>> UpdateAsync(Guid id, AssignUserToCompanyDto dto);
    Task<Result<bool>> RemoveUserFromCompanyAsync(Guid id);
}

