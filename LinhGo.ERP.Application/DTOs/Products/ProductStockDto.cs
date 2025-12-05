namespace LinhGo.ERP.Application.DTOs.Products;

public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
}

