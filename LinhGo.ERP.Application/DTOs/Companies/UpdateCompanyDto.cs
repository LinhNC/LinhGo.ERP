namespace LinhGo.ERP.Application.DTOs.Companies;

public class UpdateCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Concurrency token to prevent concurrent update conflicts.
    /// Must match the current database value for update to succeed.
    /// </summary>
    public uint Version { get; set; } 
}

