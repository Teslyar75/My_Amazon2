using DuSoleil.Domain.Enums;
using DuSoleil.Domain.Entities;
using DuSoleil.Infrastructure.Persistence;
using DuSoleil.Infrastructure.Services;
using DuSoleil.Infrastructure.Storage;
using DuSoleil.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DuSoleil.Web.Pages.Admin;

/// <summary>
/// Dashboard админки (аналог Shop/Admin из homework):
/// статистика + создание категории/товара + списки.
/// </summary>
[AdminOnly]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;

    public IndexModel(AppDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public int ProductsCount { get; private set; }
    public int CategoriesCount { get; private set; }
    public int OutOfStockCount { get; private set; }
    public int ReviewsCount { get; private set; }

    public List<Category> Categories { get; private set; } = [];
    public List<Product> Products { get; private set; } = [];

    [BindProperty]
    public CategoryForm NewCategory { get; set; } = new();

    [BindProperty]
    public ProductForm NewProduct { get; set; } = new();

    public string? Message { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateCategoryAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(NewCategory.Name) || string.IsNullOrWhiteSpace(NewCategory.Slug))
        {
            Error = "Название и slug категории обязательны.";
            return Page();
        }

        var slug = NormalizeSlug(NewCategory.Slug);
        if (await _db.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            Error = "Такой slug категории уже есть.";
            return Page();
        }

        string? imageUrl = null;
        if (NewCategory.Image is { Length: > 0 })
        {
            try
            {
                var file = _storage.Save(NewCategory.Image);
                imageUrl = "/uploads/" + file;
            }
            catch (Exception ex)
            {
                Error = "Ошибка загрузки изображения: " + ex.Message;
                return Page();
            }
        }

        _db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = NewCategory.Name.Trim(),
            Slug = slug,
            ParentCategoryId = NewCategory.ParentCategoryId,
            ImageUrl = imageUrl,
            SortOrder = NewCategory.SortOrder,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        Message = "Категория создана.";
        NewCategory = new();
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateProductAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(NewProduct.Name) || NewProduct.CategoryId == Guid.Empty)
        {
            Error = "Название и категория товара обязательны.";
            return Page();
        }

        var sku = string.IsNullOrWhiteSpace(NewProduct.Sku)
            ? "SKU-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
            : NewProduct.Sku.Trim();

        if (await _db.Products.AnyAsync(p => p.Sku == sku, cancellationToken))
        {
            Error = "SKU уже занят.";
            return Page();
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = NewProduct.Name.Trim(),
            Description = NewProduct.Description?.Trim() ?? string.Empty,
            Sku = sku,
            Slug = SlugHelper.Unique(SlugHelper.FromName(NewProduct.Name),
                s => _db.Products.Any(p => p.Slug == s)),
            Brand = NewProduct.Brand?.Trim() ?? "Du Soleil",
            CategoryId = NewProduct.CategoryId,
            Price = NewProduct.Price,
            OldPrice = NewProduct.OldPrice,
            StockQuantity = NewProduct.StockQuantity,
            Status = NewProduct.StockQuantity <= 0 ? ProductStatus.OutOfStock : ProductStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Products.Add(product);

        if (NewProduct.Image is { Length: > 0 })
        {
            try
            {
                var file = _storage.Save(NewProduct.Image);
                _db.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Url = "/uploads/" + file,
                    IsPrimary = true,
                    SortOrder = 0
                });
            }
            catch (Exception ex)
            {
                Error = "Ошибка загрузки изображения: " + ex.Message;
                return Page();
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        Message = "Товар создан.";
        NewProduct = new();
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([id], cancellationToken);
        if (product is not null)
        {
            product.Status = ProductStatus.Archived;
            product.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            Message = "Товар архивирован.";
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRestoreProductAsync(Guid id, [FromServices] IProductService products, CancellationToken cancellationToken)
    {
        await products.RestoreAsync(id, cancellationToken);
        Message = "Товар восстановлен.";
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateCategoryAsync(Guid id, [FromServices] ICategoryService categories, CancellationToken cancellationToken)
    {
        await categories.SoftDeactivateAsync(id, cancellationToken);
        Message = "Категория деактивирована (soft-delete).";
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostActivateCategoryAsync(Guid id, [FromServices] ICategoryService categories, CancellationToken cancellationToken)
    {
        await categories.ActivateAsync(id, cancellationToken);
        Message = "Категория активирована.";
        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        ProductsCount = await _db.Products.CountAsync(cancellationToken);
        CategoriesCount = await _db.Categories.CountAsync(cancellationToken);
        OutOfStockCount = await _db.Products.CountAsync(p => p.Status == ProductStatus.OutOfStock, cancellationToken);
        ReviewsCount = await _db.ProductReviews.CountAsync(cancellationToken);

        Categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        Products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    private static string NormalizeSlug(string slug) =>
        Regex.Replace(slug.Trim().ToLowerInvariant(), @"[^a-z0-9\-]+", "-").Trim('-');

    public class CategoryForm
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public int SortOrder { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class ProductForm
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public string? Brand { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; } = 1;
        public IFormFile? Image { get; set; }
    }
}
