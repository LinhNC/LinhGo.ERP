namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// Warehouse-related error codes
/// </summary>
public static class WarehouseErrors
{
    public const string NotFound = "WAREHOUSE_NOTFOUND";
    public const string CreateFailed = "WAREHOUSE_CREATE_FAILED";
    public const string UpdateFailed = "WAREHOUSE_UPDATE_FAILED";
    public const string DeleteFailed = "WAREHOUSE_DELETE_FAILED";
    public const string NameRequired = "WAREHOUSE_NAME_REQUIRED";
    public const string CodeDuplicate = "WAREHOUSE_CODE_DUPLICATE";
}

