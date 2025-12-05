namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// Order-related error codes
/// </summary>
public static class OrderErrors
{
    public const string NotFound = "ORDER_NOTFOUND";
    public const string CreateFailed = "ORDER_CREATE_FAILED";
    public const string UpdateFailed = "ORDER_UPDATE_FAILED";
    public const string DeleteFailed = "ORDER_DELETE_FAILED";
    public const string NumberDuplicate = "ORDER_NUMBER_DUPLICATE";
    public const string ItemsRequired = "ORDER_ITEMS_REQUIRED";
}

