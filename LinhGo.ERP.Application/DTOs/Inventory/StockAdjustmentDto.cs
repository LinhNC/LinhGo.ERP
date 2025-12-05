namespace LinhGo.ERP.Application.DTOs.Inventory;

public class StockAdjustmentDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string Type { get; set; } = "Adjustment";
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

