namespace LinhGo.ERP.Domain.Orders.Enums;

public enum OrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Processing = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum PaymentStatus
{
    Pending = 0,
    Partial = 1,
    Paid = 2,
    Refunded = 3,
    Cancelled = 4
}

public enum FulfillmentStatus
{
    Unfulfilled = 0,
    Partial = 1,
    Fulfilled = 2,
    Shipped = 3,
    Delivered = 4,
    Returned = 5
}

