namespace LinhGo.ERP.Application.DTOs.Companies;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // Business Information
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Industry { get; set; }
    public DateTime? EstablishedDate { get; set; }
    
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
    
    // Subscription & Status
    public bool IsActive { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    
    // Settings
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
    public string? Logo { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Concurrency token. Include this in update requests to prevent concurrent update conflicts.
    /// </summary>
    public uint Version { get; set; }
}