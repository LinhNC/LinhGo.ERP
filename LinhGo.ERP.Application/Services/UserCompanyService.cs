using AutoMapper;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Caching;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.ERP.Domain.Companies.Interfaces;
using LinhGo.ERP.Domain.Users.Entities;
using LinhGo.ERP.Domain.Users.Interfaces;
using LinhGo.SharedKernel.Cache;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Services;

/// <summary>
/// UserCompany service with distributed caching support
/// Manages user-company relationships with roles and permissions
/// Cache strategy: Cache-Aside pattern with automatic invalidation
/// Follows same architecture as CompanyService
/// </summary>
public class UserCompanyService(
    IUserCompanyRepository userCompanyRepository,
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IMapper mapper,
    ICacheService cacheService,
    ILogger<UserCompanyService> logger) : IUserCompanyService
{
    // Cache expiration times - configured for optimal performance vs freshness
    private static readonly TimeSpan ShortCacheExpiration = TimeSpan.FromMinutes(5);  // For frequently changing data
    private static readonly TimeSpan MediumCacheExpiration = TimeSpan.FromMinutes(15); // For single entities
    private static readonly TimeSpan LongCacheExpiration = TimeSpan.FromMinutes(30);   // For rarely changing lists

    /// <summary>
    /// Get user-company relationship by ID with caching
    /// </summary>
    public async Task<Result<UserCompanyDto>> GetByIdAsync(Guid id)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.User.ById(id);
            var cachedRelation = await cacheService.GetAsync<UserCompanyDto>(cacheKey);
            
            if (cachedRelation != null)
            {
                logger.LogDebug("Retrieved user-company relation {RelationId} from cache", id);
                return cachedRelation;
            }

            // Cache miss - get from database
            var relation = await userCompanyRepository.GetByIdAsync(id);
            if (relation == null)
            {
                logger.LogWarning("User-company relation with ID {RelationId} not found", id);
                return Error.WithNotFoundCode(UserCompanyErrors.NotFound, id);
            }

            var result = mapper.Map<UserCompanyDto>(relation);
            
            // Store in cache for future requests
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached user-company relation {RelationId}", id);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user-company relation with ID {RelationId}", id);
            return Error.WithFailureCode(UserCompanyErrors.GetByIdFailed);
        }
    }

    /// <summary>
    /// Get all companies for a user with caching
    /// </summary>
    public async Task<Result<IEnumerable<UserCompanyDto>>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.CompanyUser.ByUser(userId.ToString());
            var cachedRelations = await cacheService.GetAsync<List<UserCompanyDto>>(cacheKey);
            
            if (cachedRelations != null)
            {
                logger.LogDebug("Retrieved user companies for user {UserId} from cache", userId);
                return cachedRelations;
            }

            // Cache miss - get from database
            var relations = await userCompanyRepository.GetByUserIdAsync(userId);
            var result = mapper.Map<List<UserCompanyDto>>(relations) ?? [];
            
            // Store in cache
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached user companies for user {UserId}", userId);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving companies for user {UserId}", userId);
            return Error.WithFailureCode(UserCompanyErrors.GetByUserFailed);
        }
    }

    /// <summary>
    /// Get all users for a company with caching
    /// </summary>
    public async Task<Result<IEnumerable<UserCompanyDto>>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.CompanyUser.ByCompany(companyId.ToString());
            var cachedRelations = await cacheService.GetAsync<List<UserCompanyDto>>(cacheKey);
            
            if (cachedRelations != null)
            {
                logger.LogDebug("Retrieved company users for company {CompanyId} from cache", companyId);
                return cachedRelations;
            }

            // Cache miss - get from database
            var relations = await userCompanyRepository.GetByCompanyIdAsync(companyId);
            var result = mapper.Map<List<UserCompanyDto>>(relations) ?? [];
            
            // Store in cache
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached company users for company {CompanyId}", companyId);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users for company {CompanyId}", companyId);
            return Error.WithFailureCode(UserCompanyErrors.GetByCompanyFailed);
        }
    }

    /// <summary>
    /// Get specific user-company relationship with caching
    /// </summary>
    public async Task<Result<UserCompanyDto>> GetByUserAndCompanyAsync(Guid userId, Guid companyId)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeys.CompanyUser.ByUserAndCompany(userId.ToString(), companyId.ToString());
            var cachedRelation = await cacheService.GetAsync<UserCompanyDto>(cacheKey);
            
            if (cachedRelation != null)
            {
                logger.LogDebug("Retrieved user-company relation for user {UserId} and company {CompanyId} from cache", userId, companyId);
                return cachedRelation;
            }

            // Cache miss - get from database
            var relation = await userCompanyRepository.GetByUserAndCompanyAsync(userId, companyId);
            if (relation == null)
            {
                logger.LogWarning("User-company relation not found for user {UserId} and company {CompanyId}", userId, companyId);
                return Error.WithNotFoundCode(UserCompanyErrors.NotFound, userId, companyId);
            }

            var result = mapper.Map<UserCompanyDto>(relation);
            
            // Store in cache
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached user-company relation for user {UserId} and company {CompanyId}", userId, companyId);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving relation for user {UserId} and company {CompanyId}", userId, companyId);
            return Error.WithFailureCode(UserCompanyErrors.GetByIdFailed);
        }
    }

    /// <summary>
    /// Assign user to company and invalidate related caches
    /// </summary>
    public async Task<Result<UserCompanyDto>> AssignUserToCompanyAsync(AssignUserToCompanyDto dto)
    {
        try
        {
            // Verify user exists
            var userExists = await userRepository.GetByIdAsync(dto.UserId);
            if (userExists == null)
            {
                logger.LogWarning("Attempt to assign non-existent user {UserId} to company", dto.UserId);
                return Error.WithNotFoundCode(UserCompanyErrors.UserNotFound, dto.UserId);
            }

            // Verify company exists
            var companyExists = await companyRepository.GetByIdAsync(dto.CompanyId);
            if (companyExists == null)
            {
                logger.LogWarning("Attempt to assign user to non-existent company {CompanyId}", dto.CompanyId);
                return Error.WithNotFoundCode(UserCompanyErrors.CompanyNotFound, dto.CompanyId);
            }

            // Check if relationship already exists
            var existingRelation = await userCompanyRepository.GetByUserAndCompanyAsync(dto.UserId, dto.CompanyId);
            if (existingRelation != null)
            {
                logger.LogWarning("User {UserId} is already assigned to company {CompanyId}", dto.UserId, dto.CompanyId);
                return Error.WithConflictCode(UserCompanyErrors.DuplicateAssignment, dto.UserId, dto.CompanyId);
            }

            var relation = mapper.Map<UserCompany>(dto);
            relation = await userCompanyRepository.AddAsync(relation);
            
            var result = mapper.Map<UserCompanyDto>(relation);
            
            // Invalidate caches (new relationship added)
            await InvalidateRelationCachesAsync(dto.UserId, dto.CompanyId);
            logger.LogDebug("Invalidated caches after assigning user {UserId} to company {CompanyId}", dto.UserId, dto.CompanyId);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning user {UserId} to company {CompanyId}", dto.UserId, dto.CompanyId);
            return Error.WithFailureCode(UserCompanyErrors.CreateFailed);
        }
    }

    /// <summary>
    /// Update user-company relationship and invalidate related caches
    /// </summary>
    public async Task<Result<UserCompanyDto>> UpdateAsync(Guid id, AssignUserToCompanyDto dto)
    {
        try
        {
            // Fetch existing entity from database
            var existing = await userCompanyRepository.GetByIdAsync(id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to update non-existent user-company relation with ID {RelationId}", id);
                return Error.WithNotFoundCode(UserCompanyErrors.NotFound, id);
            }

            var existingUserId = existing.UserId;
            var existingCompanyId = existing.CompanyId;

            // Map DTO to existing entity
            mapper.Map(dto, existing);
            
            // Update entity
            await userCompanyRepository.UpdateAsync(existing);
            
            var result = mapper.Map<UserCompanyDto>(existing);
            
            // Invalidate caches for this relationship
            await InvalidateRelationCachesAsync(existingUserId, existingCompanyId);
            if (existingUserId != dto.UserId || existingCompanyId != dto.CompanyId)
            {
                // If user or company changed, invalidate new caches too
                await InvalidateRelationCachesAsync(dto.UserId, dto.CompanyId);
            }
            
            logger.LogDebug("Invalidated caches for updated user-company relation {RelationId}", id);
            
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict updating user-company relation with ID {RelationId}", id);
            return Error.WithConflictCode(UserCompanyErrors.ConcurrencyConflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user-company relation with ID {RelationId}", id);
            return Error.WithFailureCode(UserCompanyErrors.UpdateFailed);
        }
    }

    /// <summary>
    /// Remove user from company and invalidate related caches
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromCompanyAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Removing user-company relation {RelationId}", id);
            
            var existing = await userCompanyRepository.GetByIdAsync(id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to delete non-existent user-company relation with ID {RelationId}", id);
                return Error.WithNotFoundCode(UserCompanyErrors.NotFound, id);
            }

            var userId = existing.UserId;
            var companyId = existing.CompanyId;

            await userCompanyRepository.DeleteAsync(id);
            
            // Invalidate caches for this relationship
            await InvalidateRelationCachesAsync(userId, companyId);
            logger.LogInformation("User-company relation deleted successfully with ID {RelationId}, caches invalidated", id);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user-company relation with ID {RelationId}", id);
            return Error.WithFailureCode(UserCompanyErrors.DeleteFailed);
        }
    }

    /// <summary>
    /// Query user-company relationships with hash-based caching
    /// </summary>
    public async Task<Result<PagedResult<UserCompanyDto>>> QueryAsync(QuerierParams queries, CancellationToken ctx)
    {
        try
        {
            // Generate cache key based on query hash
            var cacheKey = CacheKeys.CompanyUser.Queries(queries);
            
            // Try to get from cache first
            var cachedResult = await cacheService.GetAsync<PagedResult<UserCompanyDto>>(cacheKey, ctx);
            
            if (cachedResult != null)
            {
                logger.LogDebug("Retrieved user-company query results from cache. Key: {CacheKey}", cacheKey);
                return cachedResult;
            }

            // Cache miss - execute search query
            var result = await userCompanyRepository.QueryAsync(queries, ctx);
            var mappedResult = new PagedResult<UserCompanyDto>
            {
                Items = mapper.Map<IEnumerable<UserCompanyDto>>(result.Items),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            
            // Cache the search result with short expiration
            await cacheService.SetAsync(cacheKey, mappedResult, ShortCacheExpiration, ctx);
            logger.LogDebug("Cached user-company query results. Key: {CacheKey}", cacheKey);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching user-company relations with queries {@Queries}", queries);
            return Error.WithFailureCode(UserCompanyErrors.QueryFailed);
        }
    }

    #region Cache Invalidation Helpers

    /// <summary>
    /// Invalidate all caches related to a specific user-company relationship
    /// Called after create, update, or delete operations
    /// </summary>
    private async Task InvalidateRelationCachesAsync(Guid userId, Guid companyId)
    {
        // Remove caches by user
        await cacheService.RemoveAsync(CacheKeys.CompanyUser.ByUser(userId.ToString()));
        
        // Remove caches by company
        await cacheService.RemoveAsync(CacheKeys.CompanyUser.ByCompany(companyId.ToString()));
        
        // Remove specific relationship cache
        await cacheService.RemoveAsync(CacheKeys.CompanyUser.ByUserAndCompany(userId.ToString(), companyId.ToString()));
        
        // Invalidate all search result caches
        await cacheService.RemoveByPatternAsync(CacheKeys.CompanyUser.Pattern());
        
        logger.LogDebug("Invalidated all user-company relation caches for user {UserId} and company {CompanyId}", userId, companyId);
    }

    #endregion
}

