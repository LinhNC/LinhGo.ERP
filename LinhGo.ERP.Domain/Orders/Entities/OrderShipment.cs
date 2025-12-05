using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Orders.Enums;

namespace LinhGo.ERP.Domain.Orders.Entities;

public class OrderShipment : TenantEntity
{
    public Guid OrderId { get; set; }
    public required string ShipmentNumber { get; set; }
    public DateTime ShipmentDate { get; set; } = DateTime.UtcNow;
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingMethod { get; set; }
    public decimal ShippingCost { get; set; } = 0;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Preparing;
    public string? Notes { get; set; }
    
    public virtual Order? Order { get; set; }
    public virtual ICollection<OrderShipmentItem>? Items { get; set; }
}


