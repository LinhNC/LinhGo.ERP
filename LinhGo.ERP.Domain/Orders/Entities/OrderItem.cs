using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Orders.Enums;

namespace LinhGo.ERP.Domain.Orders.Entities;

public class OrderItem : TenantEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TaxPercentage { get; set; } = 0;
    public decimal LineTotal { get; set; }
    public decimal QuantityShipped { get; set; } = 0;
    public decimal QuantityReturned { get; set; } = 0;
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.Unfulfilled;
    public Guid? WarehouseId { get; set; }
    public string? Notes { get; set; }
    
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
}

