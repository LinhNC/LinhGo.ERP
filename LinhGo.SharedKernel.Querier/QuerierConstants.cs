namespace LinhGo.SharedKernel.Querier;

public static class QuerierConstants
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 100;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
    
    public const string QueryFilterKeyPrefix = "filter[";
    public const string QuerySortKey = "sort";
    public const string QueryIncludeKey = "include";
    public const string QueryFieldsKey = "fields";
    public const string QuerySearchKey = "q";
    public const string QueryPageKey = "page";
    public const string QueryPageSizeKey = "pageSize";
    
    public const int MinFilterKeyLength = 8; // "filter[a]"
    public const string DefaultOperator = "eq";
}