namespace LinhGo.SharedKernel.Querier;

public class QuerierParams
{
    /// <summary>
    /// Full text search query
    /// </summary>
    public string? Q { get; set; }
    
    /// <summary>
    /// Filter conditions
    /// </summary>
    public Dictionary<string, Dictionary<string, string>>? Filters { get; set; } = new();
    
    /// <summary>
    /// Sorting criteria
    /// </summary>
    public string? Sorts { get; set; }
    
    /// <summary>
    /// Included related entities
    /// </summary>
    public string? Includes { get; set; }
    
    /// <summary>
    /// Fields to select
    /// </summary>
    public Dictionary<string, string>? Fields { get; set; } = new();
    
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }
}