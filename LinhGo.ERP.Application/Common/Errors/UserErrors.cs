namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// User-related error codes
/// </summary>
public static class UserErrors
{
    public const string NotFound = "USER_NOTFOUND";
    public const string CreateFailed = "USER_CREATE_FAILED";
    public const string UpdateFailed = "USER_UPDATE_FAILED";
    public const string DeleteFailed = "USER_DELETE_FAILED";
    public const string EmailRequired = "USER_EMAIL_REQUIRED";
    public const string EmailInvalid = "USER_EMAIL_INVALID";
    public const string EmailDuplicate = "USER_EMAIL_DUPLICATE";
    public const string PasswordRequired = "USER_PASSWORD_REQUIRED";
    public const string PasswordTooShort = "USER_PASSWORD_TOO_SHORT";
}

