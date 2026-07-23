namespace Perry.Domain.Entities;

/// <summary>
/// Медиафайл товара: фото или видео в галерее Product Page.
/// </summary>
public class ProductImage
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    /// <summary>Путь/URL к файлу (локальное хранилище или CDN).</summary>
    public string Url { get; set; } = string.Empty;

    public string? AltText { get; set; }

    /// <summary>Порядок в галерее (миниатюры слева направо).</summary>
    public int SortOrder { get; set; }

    /// <summary>Главное изображение для карточки в списке.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>true = видео (экран Product Page good's video).</summary>
    public bool IsVideo { get; set; }
}
