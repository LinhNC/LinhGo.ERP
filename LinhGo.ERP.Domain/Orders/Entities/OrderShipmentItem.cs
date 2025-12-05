using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Orders.Entities;

public class OrderShipmentItem : TenantEntity
{
    public Guid ShipmentId { get; set; }
    public Guid OrderItemId { get; set; }
    public decimal Quantity { get; set; }
    
    public virtual OrderShipment? Shipment { get; set; }
    public virtual OrderItem? OrderItem { get; set; }
}

