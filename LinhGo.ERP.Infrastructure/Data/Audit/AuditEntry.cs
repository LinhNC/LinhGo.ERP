using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LinhGo.ERP.Infrastructure.Data.Audit;

/// <summary>
/// Temporary holder for audit information during SaveChanges
/// </summary>
public class AuditEntry
{
    public EntityEntry Entry { get; }
    public string EntityName { get; set; }
    public string Action { get; set; }
    public string EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? CompanyId { get; set; }
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> AffectedColumns { get; } = new();
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
        EntityName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name;
        EntityId = string.Empty;
        Action = string.Empty;
    }
}

