namespace LinhGo.ERP.Application.DTOs.Orders;

public class OrderDetailsDto : OrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderPaymentDto> Payments { get; set; } = new();
    public List<OrderShipmentDto> Shipments { get; set; } = new();
}

