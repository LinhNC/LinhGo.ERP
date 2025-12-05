using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Inventory.Entities;

public class Stock : TenantEntity
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid? WarehouseId { get; set; }
    public decimal QuantityOnHand { get; set; } = 0;
    public decimal QuantityReserved { get; set; } = 0;
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    public decimal AverageCost { get; set; } = 0;
    public decimal TotalValue => QuantityOnHand * AverageCost;
    public DateTime? LastRestockedAt { get; set; }
    public DateTime? LastCountedAt { get; set; }
    
    public virtual Product? Product { get; set; }
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
}

