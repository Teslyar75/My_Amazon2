namespace DuSoleil.Domain.Entities;

/// <summary>
/// Пункт аккордеона «About product» на карточке товара
/// (заголовок + короткий текст, раскрывается по клику).
/// </summary>
public class ProductAboutItem
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
