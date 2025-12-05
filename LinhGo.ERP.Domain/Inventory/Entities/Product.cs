using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Inventory.Enums;

namespace LinhGo.ERP.Domain.Inventory.Entities;

public class Product : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    
    public Guid? CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public ProductType Type { get; set; } = ProductType.Goods;
    
    public decimal CostPrice { get; set; } = 0;
    public decimal SellingPrice { get; set; } = 0;
    public decimal? MinimumPrice { get; set; }
    public string? Currency { get; set; } = "USD";
    
    public string? Unit { get; set; }
    public decimal ReorderLevel { get; set; } = 0;
    public decimal ReorderQuantity { get; set; } = 0;
    public decimal MinimumStock { get; set; } = 0;
    public decimal MaximumStock { get; set; } = 0;
    
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool TrackStock { get; set; } = true;
    public bool IsSerialized { get; set; } = false;
    public bool AllowNegativeStock { get; set; } = false;
    
    public decimal? TaxRate { get; set; }
    public string? TaxCategory { get; set; }
    public string? AccountCode { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    
    public virtual ProductCategory? Category { get; set; }
    public virtual ICollection<ProductVariant>? Variants { get; set; }
    public virtual ICollection<InventoryTransaction>? Transactions { get; set; }
    public virtual ICollection<Stock>? Stocks { get; set; }
}

