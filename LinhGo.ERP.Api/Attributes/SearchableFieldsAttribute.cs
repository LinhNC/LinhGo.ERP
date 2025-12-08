namespace LinhGo.ERP.Api.Attributes;

/// <summary>
/// Attribute to specify available filter and sort fields for search operations
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SearchableFieldsAttribute : Attribute
{
    public string[] FilterFields { get; }
    public string[] SortFields { get; }
    public string EntityName { get; }

    public SearchableFieldsAttribute(string entityName, string[] filterFields, string[] sortFields)
    {
        EntityName = entityName;
        FilterFields = filterFields;
        SortFields = sortFields;
    }
}

