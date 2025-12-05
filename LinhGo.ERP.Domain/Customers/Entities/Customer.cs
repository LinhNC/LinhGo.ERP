using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Customers.Enums;

namespace LinhGo.ERP.Domain.Customers.Entities;

public class Customer : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? CompanyName { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Individual;
    
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }
    
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Industry { get; set; }
    
    public decimal CreditLimit { get; set; } = 0;
    public decimal CurrentBalance { get; set; } = 0;
    public int PaymentTermDays { get; set; } = 30;
    public string? PaymentMethod { get; set; }
    
    public bool IsActive { get; set; } = true;
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string? CustomerGroup { get; set; }
    public string? SalesRepresentative { get; set; }
    
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    
    public virtual ICollection<CustomerContact>? Contacts { get; set; }
    public virtual ICollection<CustomerAddress>? Addresses { get; set; }
}


