namespace LinhGo.SharedKernel.Cache;

/// <summary>
/// Abstraction for caching operations
/// Provides a clean interface for distributed caching with type safety
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value by key
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value or null if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set value in cache with expiration
    /// </summary>
    /// <typeparam name="T">Type of value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="absoluteExpiration">Absolute expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove cached value by key
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove all cached values matching a pattern
    /// </summary>
    /// <param name="pattern">Pattern to match (e.g., "companies:*")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh cached value by fetching new data and updating cache
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="dataFactory">Function to fetch fresh data</param>
    /// <param name="absoluteExpiration">Absolute expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refreshed value</returns>
    Task<T?> RefreshAsync<T>(string key, Func<Task<T?>> dataFactory, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class;
}