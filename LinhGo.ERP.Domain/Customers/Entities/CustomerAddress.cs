using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Customers.Entities;

public class CustomerAddress : TenantEntity
{
    public Guid CustomerId { get; set; }
    public required string AddressType { get; set; }
    public string? Label { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public virtual Customer? Customer { get; set; }
}

