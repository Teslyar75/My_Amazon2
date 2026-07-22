using DuSoleil.Domain.Entities;
using DuSoleil.Domain.Enums;
using DuSoleil.Infrastructure.Persistence;
using DuSoleil.Infrastructure.Services;
using DuSoleil.Web.Extensions;
using DuSoleil.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Web.Pages.Products;

/// <summary>
/// Каталог — Product List Page (Figma): сайдбар фильтров + сетка.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IViewedProductsService _viewed;
    private readonly ICategoryService _categories;

    public IndexModel(AppDbContext db, IViewedProductsService viewed, ICategoryService categories)
    {
        _db = db;
        _viewed = viewed;
        _categories = categories;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategorySlug { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Brand { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? MinRating { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Sort { get; set; } = "price_desc";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public string? CategoryName { get; private set; }
    public List<ProductCardVm> Products { get; private set; } = [];
    public List<ProductCardVm> ViewedProducts { get; private set; } = [];
    public List<CategoryNavVm> CategoryNav { get; private set; } = [];
    public List<string> Brands { get; private set; } = [];
    public int Total { get; private set; }
    public int TotalPages { get; private set; }
    public const int PageSize = 12;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (PageNumber < 1) PageNumber = 1;

        if (!CategoryId.HasValue && !string.IsNullOrWhiteSpace(CategorySlug))
        {
            var cat = await _categories.GetBySlugAsync(CategorySlug, cancellationToken);
            if (cat is not null)
            {
                CategoryId = cat.Id;
                CategoryName = cat.Name;
            }
        }

        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock);

        if (CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == CategoryId.Value);
            CategoryName ??= await _db.Categories
                .Where(c => c.Id == CategoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(p => p.Name.Contains(Search) || p.Brand.Contains(Search));

        if (!string.IsNullOrWhiteSpace(Brand))
            query = query.Where(p => p.Brand == Brand);

        if (MinPrice.HasValue)
            query = query.Where(p => p.Price >= MinPrice.Value);

        if (MaxPrice.HasValue)
            query = query.Where(p => p.Price <= MaxPrice.Value);

        if (MinRating.HasValue)
            query = query.Where(p => p.AverageRating >= MinRating.Value);

        query = Sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "rating_desc" => query.OrderByDescending(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.Price)
        };

        Total = await query.CountAsync(cancellationToken);
        TotalPages = Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));

        Products = await query
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .Select(p => new ProductCardVm
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
            })
            .ToListAsync(cancellationToken);

        CategoryNav = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryNavVm { Id = c.Id, Name = c.Name, Slug = c.Slug })
            .ToListAsync(cancellationToken);

        Brands = await _db.Products.AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock)
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(cancellationToken);

        ViewedProducts = (await _viewed.GetViewedProductsAsync(8, cancellationToken)).ToCardVms();
    }

    public class CategoryNavVm
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
