namespace LinhGo.ERP.Application.DTOs.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string? Unit { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public bool TrackStock { get; set; }
    public DateTime CreatedAt { get; set; }
}