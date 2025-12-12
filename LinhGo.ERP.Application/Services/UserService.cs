using AutoMapper;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Caching;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.SharedKernel.Cache;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Services;

/// <summary>
/// User service with distributed caching support
/// Cache strategy: Cache-Aside pattern with automatic invalidation
/// Follows same architecture as CompanyService
/// </summary>
public class UserService(
    IUserRepository userRepository,
    IMapper mapper,
    ICacheService cacheService,
    ILogger<UserService> logger) : IUserService
{
    // Cache expiration times - configured for optimal performance vs freshness
    private static readonly TimeSpan ShortCacheExpiration = TimeSpan.FromMinutes(5);  // For frequently changing data
    private static readonly TimeSpan MediumCacheExpiration = TimeSpan.FromMinutes(15); // For single entities
    private static readonly TimeSpan LongCacheExpiration = TimeSpan.FromMinutes(30);   // For rarely changing lists

    /// <summary>
    /// Get user by ID with caching (Cache-Aside pattern)
    /// </summary>
    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.User.ById(id);
            var cachedUser = await cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
            {
                logger.LogDebug("Retrieved user {UserId} from cache", id);
                return cachedUser;
            }

            // Cache miss - get from database
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
            {
                logger.LogWarning("User with ID {UserId} not found", id);
                return Error.WithNotFoundCode(UserErrors.NotFound, id);
            }

            var result = mapper.Map<UserDto>(user);
            
            // Store in cache for future requests
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached user {UserId}", id);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return Error.WithFailureCode(UserErrors.GetByIdFailed);
        }
    }

    /// <summary>
    /// Get all users with caching
    /// </summary>
    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync()
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.User.All();
            var cachedUsers = await cacheService.GetAsync<List<UserDto>>(cacheKey);
            
            if (cachedUsers != null)
            {
                logger.LogDebug("Retrieved all users from cache");
                return cachedUsers;
            }

            // Cache miss - get from database
            var users = await userRepository.GetAllAsync();
            var result = mapper.Map<List<UserDto>>(users) ?? [];
            
            // Store in cache with longer expiration (rarely changes)
            await cacheService.SetAsync(cacheKey, result, LongCacheExpiration);
            logger.LogDebug("Cached all users list");
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all users");
            return Error.WithFailureCode(UserErrors.GetAllFailed);
        }
    }

    /// <summary>
    /// Get active users with caching
    /// </summary>
    public async Task<Result<IEnumerable<UserDto>>> GetActiveUsersAsync()
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.User.Active();
            var cachedUsers = await cacheService.GetAsync<List<UserDto>>(cacheKey);
            
            if (cachedUsers != null)
            {
                logger.LogDebug("Retrieved active users from cache");
                return cachedUsers;
            }

            // Cache miss - get from database
            var users = await userRepository.GetActiveUsersAsync();
            var result = mapper.Map<List<UserDto>>(users) ?? [];
            
            // Store in cache with medium expiration
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached active users list");
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active users");
            return Error.WithFailureCode(UserErrors.GetActiveFailed);
        }
    }

    /// <summary>
    /// Get user by email with caching
    /// </summary>
    public async Task<Result<UserDto>> GetByEmailAsync(string email)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.User.ByEmail(email);
            var cachedUser = await cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
            {
                logger.LogDebug("Retrieved user with email {Email} from cache", email);
                return cachedUser;
            }

            // Cache miss - get from database
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                logger.LogWarning("User with email {Email} not found", email);
                return Error.WithNotFoundCode(UserErrors.NotFound, email);
            }

            var result = mapper.Map<UserDto>(user);
            
            // Store in cache
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached user with email {Email}", email);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user with email {Email}", email);
            return Error.WithFailureCode(UserErrors.GetByEmailFailed);
        }
    }

    /// <summary>
    /// Get user by username with caching
    /// </summary>
    public async Task<Result<UserDto>> GetByUsernameAsync(string username)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.User.ByUsername(username);
            var cachedUser = await cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
            {
                logger.LogDebug("Retrieved user with username {Username} from cache", username);
                return cachedUser;
            }

            // Cache miss - get from database
            var user = await userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                logger.LogWarning("User with username {Username} not found", username);
                return Error.WithNotFoundCode(UserErrors.NotFound, username);
            }

            var result = mapper.Map<UserDto>(user);
            
            // Store in cache
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached user with username {Username}", username);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user with username {Username}", username);
            return Error.WithFailureCode(UserErrors.GetByUsernameFailed);
        }
    }

    /// <summary>
    /// Create user and invalidate related caches
    /// </summary>
    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto)
    {
        try
        {
            // Check email uniqueness
            var isEmailUnique = await userRepository.IsEmailUniqueAsync(dto.Email);
            if (!isEmailUnique)
            {
                logger.LogWarning("Attempt to create duplicate user with email {Email}", dto.Email);
                return Error.WithConflictCode(UserErrors.EmailDuplicate, dto.Email);
            }

            // Check username uniqueness
            var isUsernameUnique = await userRepository.IsUsernameUniqueAsync(dto.UserName);
            if (!isUsernameUnique)
            {
                logger.LogWarning("Attempt to create duplicate user with username {Username}", dto.UserName);
                return Error.WithConflictCode(UserErrors.UsernameDuplicate, dto.UserName);
            }

            var user = mapper.Map<User>(dto);
            
            // Hash password using BCrypt before saving
            // BCrypt automatically generates and includes salt
            // WorkFactor 12 provides good security/performance balance
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);
            logger.LogDebug("Password hashed using BCrypt for user {Email}", dto.Email);
            
            user = await userRepository.AddAsync(user);
            
            var result = mapper.Map<UserDto>(user);
            
            // Invalidate list caches (new user added)
            await InvalidateListCachesAsync();
            logger.LogDebug("Invalidated list caches after creating user {UserId}", user.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error attempting to create user with email {Email}", dto.Email);
            return Error.WithFailureCode(UserErrors.CreateFailed);
        }
    }

    /// <summary>
    /// Update user and invalidate related caches
    /// </summary>
    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                logger.LogWarning("Mismatched user ID in update request: {RouteId} vs {DtoId}", id, dto.Id);
                return Error.WithValidationCode(UserErrors.IdMismatch, id, dto.Id);
            }
            
            // Fetch existing entity from database
            var existing = await userRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to update non-existent user with ID {UserId}", dto.Id);
                return Error.WithNotFoundCode(UserErrors.NotFound, dto.Id);
            }

            var existingEmail = existing.Email;
            var existingUsername = existing.UserName;

            // Map DTO to existing entity
            mapper.Map(dto, existing);
            
            // Update entity - will throw DbUpdateConcurrencyException if Version doesn't match
            await userRepository.UpdateAsync(existing);
            
            var result = mapper.Map<UserDto>(existing);
            
            // Invalidate caches for this user
            await InvalidateUserCachesAsync(dto.Id, existingEmail, existingUsername);
            logger.LogDebug("Invalidated caches for updated user {UserId}", dto.Id);
            
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Concurrency conflict: Entity was modified by another user
            logger.LogWarning(ex, "Concurrency conflict updating user with ID {UserId}", dto.Id);
            return Error.WithConflictCode(UserErrors.ConcurrencyConflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error attempting to update user with ID {UserId}", dto.Id);
            return Error.WithFailureCode(UserErrors.UpdateFailed);
        }
    }

    /// <summary>
    /// Delete user and invalidate related caches
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Deleting user {UserId}", id);
            
            var existing = await userRepository.GetByIdAsync(id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to delete non-existent user with ID {UserId}", id);
                return Error.WithNotFoundCode(UserErrors.NotFound, id);
            }

            var existingEmail = existing.Email;
            var existingUsername = existing.UserName;

            await userRepository.DeleteAsync(id);
            
            // Invalidate caches for this user
            await InvalidateUserCachesAsync(id, existingEmail, existingUsername);
            logger.LogInformation("User deleted successfully with ID {UserId}, caches invalidated", id);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return Error.WithFailureCode(UserErrors.DeleteFailed);
        }
    }

    /// <summary>
    /// Query users with hash-based caching
    /// Cache Strategy: Hash query parameters to generate unique cache key
    /// Each unique combination of filters/sorts/pagination gets its own cache entry
    /// </summary>
    public async Task<Result<PagedResult<UserDto>>> QueryAsync(QuerierParams queries, CancellationToken ctx)
    {
        try
        {
            // Generate cache key based on query hash
            var cacheKey = CacheKeys.User.Queries(queries);
            
            // Try to get from cache first
            var cachedResult = await cacheService.GetAsync<PagedResult<UserDto>>(cacheKey, ctx);
            
            if (cachedResult != null)
            {
                logger.LogDebug("Retrieved user query results from cache. Key: {CacheKey}", cacheKey);
                return cachedResult;
            }

            // Cache miss - execute search query
            var result = await userRepository.QueryAsync(queries, ctx);
            var mappedResult = new PagedResult<UserDto>
            {
                Items = mapper.Map<IEnumerable<UserDto>>(result.Items),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            
            // Cache the search result with short expiration (search data changes frequently)
            await cacheService.SetAsync(cacheKey, mappedResult, ShortCacheExpiration, ctx);
            logger.LogDebug("Cached user query results. Key: {CacheKey}", cacheKey);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying users with queries {@Queries}", queries);
            return Error.WithFailureCode(UserErrors.QueryFailed);
        }
    }

    #region Cache Invalidation Helpers

    /// <summary>
    /// Invalidate all caches related to a specific user
    /// Called after update or delete operations
    /// </summary>
    private async Task InvalidateUserCachesAsync(Guid id, string email, string username)
    {
        // Remove individual user caches
        await cacheService.RemoveAsync(CacheKeys.User.ById(id));
        await cacheService.RemoveAsync(CacheKeys.User.ByEmail(email));
        await cacheService.RemoveAsync(CacheKeys.User.ByUsername(username));
        
        // Invalidate list caches as they contain this user
        await InvalidateListCachesAsync();
    }

    /// <summary>
    /// Invalidate all list caches (all users, active users, search results)
    /// Called after create, update, or delete operations
    /// </summary>
    private async Task InvalidateListCachesAsync()
    {
        await cacheService.RemoveAsync(CacheKeys.User.All());
        await cacheService.RemoveAsync(CacheKeys.User.Active());
        
        // Invalidate all search result caches (pattern-based removal)
        // This clears all cached search queries since data has changed
        await cacheService.RemoveByPatternAsync(CacheKeys.User.Pattern());
        
        logger.LogDebug("Invalidated all user list and search caches");
    }

    #endregion
}

