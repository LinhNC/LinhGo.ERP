using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LinhGo.SharedKernel.Querier;

/// <summary>
/// Generates deterministic cache keys for search queries
/// Uses hash-based approach to handle dynamic queries with many combinations
/// Best practice: Converts query parameters into consistent hash for caching
/// </summary>
public static class QuerierKeyGenerator
{
    private const string KeyPrefix = "querier";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Generate a cache key for a search query
    /// Pattern: {entity}:search:{hash}
    /// Hash is based on: filters, sorts, page, pageSize, searchTerm
    /// </summary>
    /// <param name="entityName">Entity name (e.g., "company", "user")</param>
    /// <param name="queryParams">Search query parameters</param>
    /// <returns>Cache key like "company:search:abc123def456"</returns>
    public static string GenerateKey(string entityName, QuerierParams queryParams)
    {
        var hash = GenerateQueryHash(queryParams);
        return $"{entityName.ToLowerInvariant()}:{KeyPrefix}:{hash}";
    }

    /// <summary>
    /// Generate a pattern to match all search caches for an entity
    /// Pattern: {entity}:search:*
    /// </summary>
    public static string GeneratePattern(string entityName)
    {
        return $"{entityName.ToLowerInvariant()}:{KeyPrefix}:*";
    }
    
    /// <summary>
    /// Generate a deterministic hash from query parameters
    /// Same parameters always produce the same hash
    /// </summary>
    private static string GenerateQueryHash(QuerierParams queryParams)
    {
        // Serialize query params to JSON for consistent hashing
        var serialized = JsonSerializer.Serialize(queryParams, JsonOptions);
        
        // Generate SHA256 hash
        var bytes = Encoding.UTF8.GetBytes(serialized);
        var hashBytes = SHA256.HashData(bytes);
        
        // Convert to short hex string (first 16 characters for readability)
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }
}