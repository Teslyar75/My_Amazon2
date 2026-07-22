namespace DuSoleil.Domain.Entities;

/// <summary>
/// Корзина покупателя.
/// Для авторизованного — UserId; для гостя — SessionId (cookie/localStorage).
/// </summary>
public class Cart
{
    public Guid Id { get; set; }

    /// <summary>Пользователь из модуля Auth. Null — гостевая корзина.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Идентификатор сессии браузера для незалогиненных.</summary>
    public string? SessionId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
