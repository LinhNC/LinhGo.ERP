namespace LinhGo.ERP.Application.DTOs.Orders;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal LineTotal { get; set; }
    public decimal QuantityShipped { get; set; }
    public string FulfillmentStatus { get; set; } = string.Empty;
}

