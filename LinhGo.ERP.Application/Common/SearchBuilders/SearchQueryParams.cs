namespace LinhGo.ERP.Application.Common.SearchBuilders;

public class SearchQueryParams
{
    public string? Q { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Filter { get; set; } = new();
    public string? Sort { get; set; }
    public string? Include { get; set; }
    public Dictionary<string, string>? Fields { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record PageOptions(int Page = 1, int PageSize = 20)
{
    public int Skip => Math.Max(0, (Page - 1) * PageSize);
}