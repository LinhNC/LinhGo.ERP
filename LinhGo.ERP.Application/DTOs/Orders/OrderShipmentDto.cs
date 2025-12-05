namespace LinhGo.ERP.Application.DTOs.Orders;

public class OrderShipmentDto
{
    public Guid Id { get; set; }
    public string ShipmentNumber { get; set; } = string.Empty;
    public DateTime? ShippedDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}

