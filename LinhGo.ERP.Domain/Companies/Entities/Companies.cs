using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Companies.Entities;

/// <summary>
/// Represents a company/tenant in the multi-tenant ERP system
/// Each company has its own isolated data
/// </summary>
public class Company : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; } // Unique company identifier
    public string? TaxId { get; set; } // Tax identification number
    public string? RegistrationNumber { get; set; }
    
    // Contact Information
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    
    // Address Information
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Business Information
    public string? Industry { get; set; }
    public DateTime? EstablishedDate { get; set; }
    
    // Subscription & Status
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    
    // Settings
    public string? Currency { get; set; } = "USD";
    public string? TimeZone { get; set; }
    public string? Language { get; set; } = "en";
    public string? Logo { get; set; } // URL or path to logo
    
    // Navigation Properties
    public virtual ICollection<CompanySettings>? Settings { get; set; }
}