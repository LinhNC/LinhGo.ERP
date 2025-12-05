namespace LinhGo.ERP.Application.DTOs.Orders;

public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

