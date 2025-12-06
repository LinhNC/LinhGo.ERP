namespace LinhGo.ERP.Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? CompanyId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class AuditLogDetailDto : AuditLogDto
{
    public Dictionary<string, object?>? OldValuesObject { get; set; }
    public Dictionary<string, object?>? NewValuesObject { get; set; }
    public List<PropertyChangeDto>? PropertyChanges { get; set; }
}

public class PropertyChangeDto
{
    public string PropertyName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}

public class AuditLogQueryDto
{
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public string? UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

