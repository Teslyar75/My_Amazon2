namespace DuSoleil.Domain.Entities;

/// <summary>
/// Позиция заказа со снимком данных товара на момент покупки (homework OrderItem).
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;

    public string? ProductDescription { get; set; }

    public decimal ProductPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public string? ProductImageUrl { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
