namespace Perry.Domain.Entities;

/// <summary>
/// Одна позиция в корзине: товар + количество
/// (экраны Cart V2 в Figma).
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }

    public Cart Cart { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    /// <summary>Количество единиц товара.</summary>
    public int Quantity { get; set; } = 1;

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
