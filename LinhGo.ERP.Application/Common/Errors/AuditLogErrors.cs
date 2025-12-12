namespace LinhGo.ERP.Application.Common.Errors;

/// <summary>
/// AuditLog-related error codes
/// </summary>
public static class AuditLogErrors
{
    // Query errors
    public const string NotFound = "AUDITLOG_NOTFOUND";
    public const string GetByIdFailed = "AUDITLOG_GET_ID_FAILED";
    public const string GetPagedFailed = "AUDITLOG_GET_PAGED_FAILED";
    public const string GetByEntityFailed = "AUDITLOG_GET_ENTITY_FAILED";
    public const string GetByCompanyFailed = "AUDITLOG_GET_COMPANY_FAILED";
    public const string GetByUserFailed = "AUDITLOG_GET_USER_FAILED";
    public const string QueryFailed = "AUDITLOG_QUERY_FAILED";
    
    // Command errors
    public const string CreateFailed = "AUDITLOG_CREATE_FAILED";
    
    // Validation errors
    public const string InvalidDateRange = "AUDITLOG_INVALID_DATE_RANGE";
}

