using DuSoleil.Domain.Enums;

namespace DuSoleil.Domain.Entities;

/// <summary>
/// Заказ покупателя (из homework Order + под Figma Admin Order).
/// </summary>
public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public int ItemsCount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Completed;

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
