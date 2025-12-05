namespace LinhGo.ERP.Application.DTOs.Products;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal PriceAdjustment { get; set; }
    public bool IsActive { get; set; }
}

