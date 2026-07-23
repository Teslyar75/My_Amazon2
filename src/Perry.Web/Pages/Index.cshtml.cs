using Perry.Domain.Entities;
using Perry.Domain.Enums;
using Perry.Infrastructure.Persistence;
using Perry.Infrastructure.Services;
using Perry.Web.Extensions;
using Perry.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Perry.Web.Pages;

/// <summary>
/// Главная страница (Desktop - Main в Figma):
/// hero, категории, Trending deals, Best sellers, Recently viewed.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IViewedProductsService _viewed;

    public IndexModel(AppDbContext db, IViewedProductsService viewed)
    {
        _db = db;
        _viewed = viewed;
    }

    public List<CategorySpotVm> CategorySpots { get; private set; } = [];
    public List<ProductCardVm> TrendingDeals { get; private set; } = [];
    public List<ProductCardVm> BestSellers { get; private set; } = [];
    public List<ProductCardVm> ViewedProducts { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        CategorySpots = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.SortOrder)
            .Take(6)
            .Select(c => new CategorySpotVm
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl
            })
            .ToListAsync(cancellationToken);

        if (CategorySpots.Count < 4)
        {
            CategorySpots = await _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Take(6)
                .Select(c => new CategorySpotVm
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync(cancellationToken);
        }

        var visible = _db.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock);

        TrendingDeals = await MapCards(
            visible.Where(p => p.OldPrice != null).OrderByDescending(p => p.ReviewCount).Take(8),
            cancellationToken);

        BestSellers = await MapCards(
            visible.Where(p => p.IsBestSeller).OrderByDescending(p => p.AverageRating).Take(8),
            cancellationToken);

        if (TrendingDeals.Count == 0)
            TrendingDeals = await MapCards(visible.OrderByDescending(p => p.CreatedAtUtc).Take(8), cancellationToken);

        if (BestSellers.Count == 0)
            BestSellers = await MapCards(visible.OrderByDescending(p => p.AverageRating).Take(8), cancellationToken);

        ViewedProducts = (await _viewed.GetViewedProductsAsync(8, cancellationToken)).ToCardVms();
    }

    private static async Task<List<ProductCardVm>> MapCards(
        IQueryable<Product> query,
        CancellationToken cancellationToken)
    {
        return await query.Select(p => new ProductCardVm
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
            ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
                ?? p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
        }).ToListAsync(cancellationToken);
    }

    public class CategorySpotVm
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
