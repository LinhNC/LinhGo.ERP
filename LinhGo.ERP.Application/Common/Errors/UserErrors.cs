namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// User-related error codes
/// </summary>
public static class UserErrors
{
    // Query errors
    public const string NotFound = "USER_NOTFOUND";
    public const string GetByIdFailed = "USER_GET_ID_FAILED";
    public const string GetAllFailed = "USER_GET_ALL_FAILED";
    public const string GetActiveFailed = "USER_GET_ACTIVE_FAILED";
    public const string GetByEmailFailed = "USER_GET_EMAIL_FAILED";
    public const string GetByUsernameFailed = "USER_GET_USERNAME_FAILED";
    public const string QueryFailed = "USER_SEARCH_FAILED";
    
    // Command errors
    public const string CreateFailed = "USER_CREATE_FAILED";
    public const string UpdateFailed = "USER_UPDATE_FAILED";
    public const string DeleteFailed = "USER_DELETE_FAILED";
    
    // Validation errors
    public const string EmailRequired = "USER_EMAIL_REQUIRED";
    public const string EmailInvalid = "USER_EMAIL_INVALID";
    public const string EmailDuplicate = "USER_EMAIL_DUPLICATE";
    public const string UsernameDuplicate = "USER_USERNAME_DUPLICATE";
    public const string PasswordRequired = "USER_PASSWORD_REQUIRED";
    public const string PasswordTooShort = "USER_PASSWORD_TOO_SHORT";
    public const string IdMismatch = "USER_ID_MISMATCH";
    public const string ConcurrencyConflict = "USER_CONCURRENCY_CONFLICT";
}

/// <summary>
/// UserCompany-related error codes
/// </summary>
public static class UserCompanyErrors
{
    // Query errors
    public const string NotFound = "USERCOMPANY_NOTFOUND";
    public const string GetByIdFailed = "USERCOMPANY_GET_ID_FAILED";
    public const string GetByUserFailed = "USERCOMPANY_GET_USER_FAILED";
    public const string GetByCompanyFailed = "USERCOMPANY_GET_COMPANY_FAILED";
    public const string QueryFailed = "USERCOMPANY_SEARCH_FAILED";
    
    // Command errors
    public const string CreateFailed = "USERCOMPANY_CREATE_FAILED";
    public const string UpdateFailed = "USERCOMPANY_UPDATE_FAILED";
    public const string DeleteFailed = "USERCOMPANY_DELETE_FAILED";
    
    // Validation errors
    public const string DuplicateAssignment = "USERCOMPANY_DUPLICATE_ASSIGNMENT";
    public const string IdMismatch = "USERCOMPANY_ID_MISMATCH";
    public const string ConcurrencyConflict = "USERCOMPANY_CONCURRENCY_CONFLICT";
    public const string UserNotFound = "USERCOMPANY_USER_NOT_FOUND";
    public const string CompanyNotFound = "USERCOMPANY_COMPANY_NOT_FOUND";
}

