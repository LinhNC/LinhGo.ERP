namespace LinhGo.ERP.Application.DTOs.Inventory;

public class StockTransferDto
{
    public Guid ProductId { get; set; }
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

