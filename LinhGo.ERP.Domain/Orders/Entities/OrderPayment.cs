using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Orders.Enums;

namespace LinhGo.ERP.Domain.Orders.Entities;

public class OrderPayment : TenantEntity
{
    public Guid OrderId { get; set; }
    public required string PaymentNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public required string PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public PaymentStatusType Status { get; set; } = PaymentStatusType.Completed;
    public string? Notes { get; set; }
    
    public virtual Order? Order { get; set; }
}


