namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// Customer-related error codes
/// </summary>
public static class CustomerErrors
{
    public const string NotFound = "CUSTOMER_NOTFOUND";
    public const string CreateFailed = "CUSTOMER_CREATE_FAILED";
    public const string UpdateFailed = "CUSTOMER_UPDATE_FAILED";
    public const string DeleteFailed = "CUSTOMER_DELETE_FAILED";
    public const string NameRequired = "CUSTOMER_NAME_REQUIRED";
    public const string CodeDuplicate = "CUSTOMER_CODE_DUPLICATE";
}

