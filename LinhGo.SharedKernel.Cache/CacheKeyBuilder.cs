namespace LinhGo.SharedKernel.Cache;

/// <summary>
/// Fluent builder for constructing cache keys with best practices
/// Provides type-safe, consistent cache key generation for all entities
/// Pattern: {entity}:{operation}:{identifier}
/// </summary>
public sealed class CacheKeyBuilder
{
    private string? _entity;
    private string? _operation;
    private string? _identifier;
    private readonly List<string> _tags = new();

    private CacheKeyBuilder() { }

    /// <summary>
    /// Start building a cache key for an entity
    /// </summary>
    /// <param name="entityName">Entity name (e.g., "company", "user", "product")</param>
    public static CacheKeyBuilder ForEntity(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be empty", nameof(entityName));

        return new CacheKeyBuilder
        {
            _entity = entityName.ToLowerInvariant()
        };
    }

    /// <summary>
    /// Specify the operation type (e.g., "id", "code", "email", "all")
    /// </summary>
    public CacheKeyBuilder WithOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operation cannot be empty", nameof(operation));

        _operation = operation.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Specify the identifier (e.g., guid, code, email)
    /// </summary>
    public CacheKeyBuilder WithIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));

        _identifier = identifier;
        return this;
    }

    /// <summary>
    /// Specify the identifier using a GUID
    /// </summary>
    public CacheKeyBuilder WithId(Guid id)
    {
        _identifier = id.ToString();
        return this;
    }

    /// <summary>
    /// Add a tag for grouping (optional)
    /// </summary>
    public CacheKeyBuilder WithTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            _tags.Add(tag.ToLowerInvariant());
        return this;
    }

    /// <summary>
    /// Build the cache key
    /// </summary>
    public string Build()
    {
        if (string.IsNullOrEmpty(_entity))
            throw new InvalidOperationException("Entity must be specified");

        if (string.IsNullOrEmpty(_operation))
            throw new InvalidOperationException("Operation must be specified");

        var keyParts = new List<string> { _entity, _operation };

        if (!string.IsNullOrEmpty(_identifier))
            keyParts.Add(_identifier);

        if (_tags.Count > 0)
            keyParts.AddRange(_tags);

        return string.Join(":", keyParts);
    }

    /// <summary>
    /// Implicit conversion to string
    /// </summary>
    public static implicit operator string(CacheKeyBuilder builder) => builder.Build();

    /// <summary>
    /// Build a pattern for cache removal (e.g., "company:*")
    /// </summary>
    public string BuildPattern()
    {
        if (string.IsNullOrEmpty(_entity))
            throw new InvalidOperationException("Entity must be specified");

        var keyParts = new List<string> { _entity };

        if (!string.IsNullOrEmpty(_operation))
            keyParts.Add(_operation);

        keyParts.Add("*");

        return string.Join(":", keyParts);
    }
}
