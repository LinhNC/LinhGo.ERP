namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// Company-related error codes
/// </summary>
public static class CompanyErrors
{
    public const string NotFound = "COMPANY_NOTFOUND";
    public const string CreateFailed = "COMPANY_CREATE_FAILED";
    public const string UpdateFailed = "COMPANY_UPDATE_FAILED";
    public const string DeleteFailed = "COMPANY_DELETE_FAILED";
    public const string GetByIdFailed = "COMPANY_GET_ID_FAILED";
    public const string GetAllFailed = "COMPANY_GET_ALL_FAILED";
    public const string GetActiveFailed = "COMPANY_GET_ACTIVE_FAILED";
    public const string GetByCodeFailed = "COMPANY_GET_CODE_FAILED";
    public const string NameRequired = "COMPANY_NAME_REQUIRED";
    public const string NameTooLong = "COMPANY_NAME_TOO_LONG";
    public const string CodeDuplicate = "COMPANY_CODE_DUPLICATE";
    public const string DuplicateCode = "COMPANY_DUPLICATE_CODE";
}

