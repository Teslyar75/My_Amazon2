namespace Perry.Domain.Enums;

/// <summary>Статусы заказа (Admin Order + история покупателя).</summary>
public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Shipped = 2,
    Completed = 3,
    Cancelled = 4
}
