using Perry.Domain.Entities;
using Perry.Web.ViewModels;

namespace Perry.Web.Extensions;

/// <summary>Маппинг Product → карточка витрины.</summary>
public static class ProductCardMapping
{
    public static ProductCardVm ToCardVm(this Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Brand = p.Brand,
        Slug = p.Slug,
        Price = p.Price,
        OldPrice = p.OldPrice,
        DiscountPercent = p.OldPrice != null && p.OldPrice > p.Price
            ? (int?)Math.Round((p.OldPrice.Value - p.Price) / p.OldPrice.Value * 100)
            : null,
        AverageRating = p.AverageRating,
        ReviewCount = p.ReviewCount,
        IsBestSeller = p.IsBestSeller,
        Status = p.Status,
        ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url
            ?? p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
    };

    public static List<ProductCardVm> ToCardVms(this IEnumerable<Product> products) =>
        products.Select(ToCardVm).ToList();
}
