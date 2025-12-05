namespace LinhGo.ERP.Application.DTOs.Orders;

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public DateTime? PaymentDate { get; set; }
}

