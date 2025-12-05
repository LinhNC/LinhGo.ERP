using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Orders.Enums;

namespace LinhGo.ERP.Domain.Orders.Entities;

public class Order : TenantEntity
{
    public required string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    
    public string? BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    
    public string? ShippingAddressLine1 { get; set; }
    public string? ShippingAddressLine2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    
    public decimal SubTotal { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TaxPercentage { get; set; } = 0;
    public decimal ShippingCost { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public decimal PaidAmount { get; set; } = 0;
    public decimal BalanceAmount => TotalAmount - PaidAmount;
    public string? Currency { get; set; } = "USD";
    
    public string? PaymentMethod { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime? PaymentDueDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    public string? ShippingMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.Unfulfilled;
    
    public Guid? SalesRepresentativeId { get; set; }
    public string? SalesChannel { get; set; }
    
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? Tags { get; set; }
    
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderItem>? Items { get; set; }
    public virtual ICollection<OrderPayment>? Payments { get; set; }
    public virtual ICollection<OrderShipment>? Shipments { get; set; }
}


