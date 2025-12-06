namespace LinhGo.ERP.Application.DTOs.Companies;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Currency { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Concurrency token. Include this in update requests to prevent concurrent update conflicts.
    /// </summary>
    public uint Version { get; set; }
}