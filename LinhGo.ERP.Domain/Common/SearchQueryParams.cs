namespace LinhGo.ERP.Domain.Common;

public class SearchQueryParams
{
    public string? Q { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Filters { get; set; } = new();
    public string? Sorts { get; set; }
    public string? Includes { get; set; }
    public Dictionary<string, string>? Fields { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}