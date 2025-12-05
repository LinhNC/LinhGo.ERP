namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// Product-related error codes
/// </summary>
public static class ProductErrors
{
    public const string NotFound = "PRODUCT_NOTFOUND";
    public const string CreateFailed = "PRODUCT_CREATE_FAILED";
    public const string UpdateFailed = "PRODUCT_UPDATE_FAILED";
    public const string DeleteFailed = "PRODUCT_DELETE_FAILED";
    public const string NameRequired = "PRODUCT_NAME_REQUIRED";
    public const string SkuDuplicate = "PRODUCT_SKU_DUPLICATE";
    public const string InsufficientStock = "PRODUCT_INSUFFICIENT_STOCK";
}

