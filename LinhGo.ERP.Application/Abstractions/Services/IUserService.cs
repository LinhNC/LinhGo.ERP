using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.ERP.Domain.Common;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Application.Abstractions.Services;

/// <summary>
/// User service interface
/// Provides CRUD operations and caching for User entity
/// </summary>
public interface IUserService
{
    // Query operations
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<UserDto>>> GetAllAsync();
    Task<Result<IEnumerable<UserDto>>> GetActiveUsersAsync();
    Task<Result<UserDto>> GetByEmailAsync(string email);
    Task<Result<UserDto>> GetByUsernameAsync(string username);
    Task<Result<PagedResult<UserDto>>> QueryAsync(QuerierParams queries, CancellationToken ctx);
    
    // Command operations
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto);
    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}

