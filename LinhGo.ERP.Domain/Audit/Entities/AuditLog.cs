using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Audit.Entities;

/// <summary>
/// Stores detailed audit trail of all entity changes
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// The name of the entity/table that was changed
    /// </summary>
    public required string EntityName { get; set; }
    
    /// <summary>
    /// The ID of the entity that was changed
    /// </summary>
    public required string EntityId { get; set; }
    
    /// <summary>
    /// The type of change: Create, Update, Delete
    /// </summary>
    public required string Action { get; set; }
    
    /// <summary>
    /// Timestamp when the change occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who made the change
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Username of the user who made the change
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Company/Tenant ID for multi-tenant isolation
    /// </summary>
    public Guid? CompanyId { get; set; }
    
    /// <summary>
    /// JSON representation of the old values (before change)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// JSON representation of the new values (after change)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// List of properties that were changed
    /// </summary>
    public string? AffectedColumns { get; set; }
    
    /// <summary>
    /// Primary key values (for composite keys)
    /// </summary>
    public string? PrimaryKey { get; set; }
    
    /// <summary>
    /// IP Address of the client that made the change
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent / browser information
    /// </summary>
    public string? UserAgent { get; set; }
}

