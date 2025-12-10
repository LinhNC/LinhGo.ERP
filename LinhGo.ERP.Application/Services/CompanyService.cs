using AutoMapper;
using LinhGo.ERP.Application.Abstractions.Caching;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Caching;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Companies;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Companies.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Services;

/// <summary>
/// Company service with distributed caching support
/// Cache strategy: Cache-Aside pattern with automatic invalidation
/// </summary>
public class CompanyService(
    ICompanyRepository companyRepository,
    IMapper mapper,
    ICacheService cacheService,
    ILogger<CompanyService> logger) : ICompanyService
{
    // Cache expiration times - configured for optimal performance vs freshness
    private static readonly TimeSpan ShortCacheExpiration = TimeSpan.FromMinutes(5);  // For frequently changing data
    private static readonly TimeSpan MediumCacheExpiration = TimeSpan.FromMinutes(15); // For single entities
    private static readonly TimeSpan LongCacheExpiration = TimeSpan.FromMinutes(30);   // For rarely changing lists

    /// <summary>
    /// Get company by ID with caching (Cache-Aside pattern)
    /// </summary>
    public async Task<Result<CompanyDto>> GetByIdAsync(Guid id)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeyFactory.Company.ById(id);
            var cachedCompany = await cacheService.GetAsync<CompanyDto>(cacheKey);
            
            if (cachedCompany != null)
            {
                logger.LogDebug("Retrieved company {CompanyId} from cache", id);
                return cachedCompany;
            }

            // Cache miss - get from database
            var company = await companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                logger.LogWarning("Company with ID {CompanyId} not found", id);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, id);
            }

            var result = mapper.Map<CompanyDto>(company);
            
            // Store in cache for future requests
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached company {CompanyId}", id);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving company with ID {CompanyId}", id);
            return Error.WithFailureCode(CompanyErrors.GetByIdFailed);
        }
    }
    /// <summary>
    /// Get all companies with caching
    /// </summary>
    public async Task<Result<IEnumerable<CompanyDto>>> GetAllAsync()
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeyFactory.Company.All();
            var cachedCompanies = await cacheService.GetAsync<List<CompanyDto>>(cacheKey);
            
            if (cachedCompanies != null)
            {
                logger.LogDebug("Retrieved all companies from cache");
                return cachedCompanies;
            }

            // Cache miss - get from database
            var companies = await companyRepository.GetAllAsync();
            var result = mapper.Map<List<CompanyDto>>(companies) ?? [];
            
            // Store in cache with longer expiration (rarely changes)
            await cacheService.SetAsync(cacheKey, result, LongCacheExpiration);
            logger.LogDebug("Cached all companies list");
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all companies");
            return Error.WithFailureCode(CompanyErrors.GetAllFailed);
        }
    }

    /// <summary>
    /// Get active companies with caching
    /// </summary>
    public async Task<Result<IEnumerable<CompanyDto>>> GetActiveCompaniesAsync()
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeyFactory.Company.Active();
            var cachedCompanies = await cacheService.GetAsync<List<CompanyDto>>(cacheKey);
            
            if (cachedCompanies != null)
            {
                logger.LogDebug("Retrieved active companies from cache");
                return cachedCompanies;
            }

            // Cache miss - get from database
            var companies = await companyRepository.GetActiveCompaniesAsync();
            var result = mapper.Map<List<CompanyDto>>(companies) ?? [];
            
            // Store in cache with medium expiration
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached active companies list");
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active companies");
            return Error.WithFailureCode(CompanyErrors.GetActiveFailed);
        }
    }

    /// <summary>
    /// Get company by code with caching
    /// </summary>
    public async Task<Result<CompanyDto>> GetByCodeAsync(string code)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = CacheKeyFactory.Company.ByCode(code);
            var cachedCompany = await cacheService.GetAsync<CompanyDto>(cacheKey);
            
            if (cachedCompany != null)
            {
                logger.LogDebug("Retrieved company with code {Code} from cache", code);
                return cachedCompany;
            }

            // Cache miss - get from database
            var company = await companyRepository.GetByCodeAsync(code);
            if (company == null)
            {
                logger.LogWarning("Company with code {Code} not found", code);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, code);
            }

            var result = mapper.Map<CompanyDto>(company);
            
            // Store in cache
            await cacheService.SetAsync(cacheKey, result, MediumCacheExpiration);
            logger.LogDebug("Cached company with code {Code}", code);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving company with code {Code}", code);
            return Error.WithFailureCode(CompanyErrors.GetByCodeFailed);
        }
    }

    /// <summary>
    /// Create company and invalidate related caches
    /// </summary>
    public async Task<Result<CompanyDto>> CreateAsync(CreateCompanyDto dto)
    {
        try
        {
            var isUnique = await companyRepository.IsCodeUniqueAsync(dto.Code);
            if (!isUnique)
            {
                logger.LogWarning("Attempt to create duplicate company with code {Code}", dto.Code);
                return Error.WithConflictCode(CompanyErrors.DuplicateCode, dto.Code);
            }

            var company = mapper.Map<Company>(dto);
            company = await companyRepository.AddAsync(company);
            
            var result = mapper.Map<CompanyDto>(company);
            
            // Invalidate list caches (new company added)
            await InvalidateListCachesAsync();
            logger.LogDebug("Invalidated list caches after creating company {CompanyId}", company.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error attempting to create company with code {Code}", dto.Code);
            return Error.WithFailureCode(CompanyErrors.CreateFailed);
        }
    }

    /// <summary>
    /// Update company and invalidate related caches
    /// </summary>
    public async Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                logger.LogWarning("Mismatched company ID in update request: {RouteId} vs {DtoId}", id, dto.Id);
                return Error.WithValidationCode(CompanyErrors.IdMismatch, id, dto.Id);
            }
            
            // Fetch existing entity from database
            var existing = await companyRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to update non-existent company with ID {CompanyId}", dto.Id);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, dto.Id);
            }

            var existingCode = existing.Code; // Store for cache invalidation

            // Map DTO to existing entity (preserves Code and other unmapped fields)
            // Version from DTO will be used for concurrency check
            mapper.Map(dto, existing);
            
            // Update entity - will throw DbUpdateConcurrencyException if Version doesn't match database
            await companyRepository.UpdateAsync(existing);
            
            var result = mapper.Map<CompanyDto>(existing);
            
            // Invalidate caches for this company
            await InvalidateCompanyCachesAsync(dto.Id, existingCode);
            logger.LogDebug("Invalidated caches for updated company {CompanyId}", dto.Id);
            
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Concurrency conflict: Entity was modified by another user
            logger.LogWarning(ex, "Concurrency conflict updating company with ID {CompanyId}", dto.Id);
            return Error.WithConflictCode(CompanyErrors.ConcurrencyConflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error attempting to update company with ID {CompanyId}", dto.Id);
            return Error.WithFailureCode(CompanyErrors.UpdateFailed);
        }
    }

    /// <summary>
    /// Delete company and invalidate related caches
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Deleting company {CompanyId}", id);
            
            var existing = await companyRepository.GetByIdAsync(id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to delete non-existent company with ID {CompanyId}", id);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, id);
            }

            var existingCode = existing.Code; // Store for cache invalidation

            await companyRepository.DeleteAsync(id);
            
            // Invalidate caches for this company
            await InvalidateCompanyCachesAsync(id, existingCode);
            logger.LogInformation("Company deleted successfully with ID {CompanyId}, caches invalidated", id);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting company with ID {CompanyId}", id);
            return Error.WithFailureCode(CompanyErrors.DeleteFailed);
        }
    }

    /// <summary>
    /// Search companies with hash-based caching
    /// Cache Strategy: Hash query parameters to generate unique cache key
    /// Each unique combination of filters/sorts/pagination gets its own cache entry
    /// </summary>
    public async Task<Result<PagedResult<CompanyDto>>> SearchAsync(SearchQueryParams queries, CancellationToken ctx)
    {
        try
        {
            // Generate cache key based on query hash
            var cacheKey = CacheKeyFactory.Company.Search(queries);
            
            // Try to get from cache first
            var cachedResult = await cacheService.GetAsync<PagedResult<CompanyDto>>(cacheKey, ctx);
            
            if (cachedResult != null)
            {
                logger.LogDebug("Retrieved company search results from cache. Key: {CacheKey}", cacheKey);
                return cachedResult;
            }

            // Cache miss - execute search query
            var result = await companyRepository.SearchAsync(queries, ctx);
            var mappedResult = new PagedResult<CompanyDto>
            {
                Items = mapper.Map<IEnumerable<CompanyDto>>(result.Items),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            
            // Cache the search result with short expiration (search data changes frequently)
            await cacheService.SetAsync(cacheKey, mappedResult, ShortCacheExpiration, ctx);
            logger.LogDebug("Cached company search results. Key: {CacheKey}", cacheKey);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching companies with queries {@Queries}", queries);
            return Error.WithFailureCode(CompanyErrors.SearchFailed);
        }
    }

    #region Cache Invalidation Helpers

    /// <summary>
    /// Invalidate all caches related to a specific company
    /// Called after update or delete operations
    /// </summary>
    private async Task InvalidateCompanyCachesAsync(Guid id, string code)
    {
        // Remove individual company caches
        await cacheService.RemoveAsync(CacheKeyFactory.Company.ById(id));
        await cacheService.RemoveAsync(CacheKeyFactory.Company.ByCode(code));
        
        // Invalidate list caches as they contain this company
        await InvalidateListCachesAsync();
    }

    /// <summary>
    /// Invalidate all list caches (all companies, active companies, search results)
    /// Called after create, update, or delete operations
    /// </summary>
    private async Task InvalidateListCachesAsync()
    {
        await cacheService.RemoveAsync(CacheKeyFactory.Company.All());
        await cacheService.RemoveAsync(CacheKeyFactory.Company.Active());
        
        // Invalidate all search result caches (pattern-based removal)
        // This clears all cached search queries since data has changed
        await cacheService.RemoveByPatternAsync(CacheKeyFactory.Company.PatternSearch());
        
        logger.LogDebug("Invalidated all company list and search caches");
    }

    #endregion
}
