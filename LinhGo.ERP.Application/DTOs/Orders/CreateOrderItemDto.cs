namespace LinhGo.ERP.Application.DTOs.Orders;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
    public Guid? WarehouseId { get; set; }
}

