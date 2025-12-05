using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Customers.Entities;

public class CustomerContact : TenantEntity
{
    public Guid CustomerId { get; set; }
    public required string Name { get; set; }
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    public virtual Customer? Customer { get; set; }
}

