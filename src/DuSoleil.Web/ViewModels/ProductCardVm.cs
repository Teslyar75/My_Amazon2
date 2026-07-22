using DuSoleil.Domain.Enums;

namespace DuSoleil.Web.ViewModels;

/// <summary>
/// Краткая карточка товара для списков и каруселей на витрине
/// (главная, каталог, блоки «You may also like»).
/// </summary>
public class ProductCardVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }

    /// <summary>Процент скидки для бейджа «-25%». Null — без скидки.</summary>
    public int? DiscountPercent { get; set; }

    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsBestSeller { get; set; }
    public ProductStatus Status { get; set; }

    /// <summary>URL главного изображения (или null, если картинок нет).</summary>
    public string? ImageUrl { get; set; }
}
