using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Inventory.Enums;

namespace LinhGo.ERP.Domain.Inventory.Entities;

public class InventoryTransaction : TenantEntity
{
    public required string TransactionNumber { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid? FromWarehouseId { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; } = 0;
    public decimal TotalCost => Quantity * UnitCost;
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? Reason { get; set; }
    
    public virtual Product? Product { get; set; }
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Warehouse? FromWarehouse { get; set; }
    public virtual Warehouse? ToWarehouse { get; set; }
}


