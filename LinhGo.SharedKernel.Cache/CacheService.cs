using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LinhGo.SharedKernel.Cache;

/// <summary>
/// Distributed cache service implementation using IDistributedCache
/// Supports Redis, or in-memory distributed cache
/// </summary>
internal sealed class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<CacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cachedData, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached data for key: {CacheKey}", key);
            return null; // Fail gracefully - don't break the application if cache fails
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(value, JsonOptions);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(10) // Default 10 minutes
            };

            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
            
            _logger.LogDebug("Cached data for key: {CacheKey} with expiration: {Expiration}", 
                key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching data for key: {CacheKey}", key);
            // Fail gracefully - cache failures should not break the application
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cached data for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached data for key: {CacheKey}", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal is not natively supported by IDistributedCache
        // This is a placeholder - for production, use Redis-specific implementation or key tracking
        _logger.LogWarning("Pattern-based cache removal is not implemented for IDistributedCache. Pattern: {Pattern}", pattern);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<T?> RefreshAsync<T>(string key, Func<Task<T?>> dataFactory, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _logger.LogDebug("Refreshing cache for key: {CacheKey}", key);
            
            // Remove existing cache
            await RemoveAsync(key, cancellationToken);
            
            // Fetch fresh data
            var freshData = await dataFactory();
            
            if (freshData == null)
            {
                _logger.LogDebug("Data factory returned null for key: {CacheKey}, not caching", key);
                return null;
            }
            
            // Store fresh data in cache
            await SetAsync(key, freshData, absoluteExpiration, cancellationToken);
            
            _logger.LogDebug("Successfully refreshed cache for key: {CacheKey}", key);
            return freshData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache for key: {CacheKey}", key);
            return null;
        }
    }
}