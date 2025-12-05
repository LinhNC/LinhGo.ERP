namespace LinhGo.ERP.Application.DTOs.Inventory;

public class CreateWarehouseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public bool IsDefault { get; set; }
}

