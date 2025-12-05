namespace LinhGo.ERP.Application.DTOs.Products;

public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public string Type { get; set; } = "Goods";
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string? Unit { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool TrackStock { get; set; } = true;
}

