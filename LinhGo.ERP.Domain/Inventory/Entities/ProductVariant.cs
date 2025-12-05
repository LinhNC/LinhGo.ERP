using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Inventory.Entities;

public class ProductVariant : TenantEntity
{
    public Guid ProductId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Style { get; set; }
    public string? Attributes { get; set; }
    public decimal? PriceAdjustment { get; set; } = 0;
    public decimal? CostAdjustment { get; set; } = 0;
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual Product? Product { get; set; }
}

