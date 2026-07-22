using DuSoleil.Domain.Enums;
using DuSoleil.Infrastructure.Persistence;
using DuSoleil.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Api.Controllers;

/// <summary>
/// API товаров (витрина).
/// Маршруты:
///   GET /api/products      — список (Product List Page)
///   GET /api/products/{id} — карточка (Product Page)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Список товаров с фильтрами, сортировкой и пагинацией
    /// (экран Product List в Figma: бренд, цена, рейтинг, поиск).
    /// Пример: GET /api/products?brand=Roku&amp;minPrice=10&amp;page=1&amp;sort=price_asc
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? brand,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minRating,
        [FromQuery] string? search,
        [FromQuery] string sort = "price_desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        // Защита от некорректных параметров пагинации
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 12;

        // Черновики и архив покупателю не показываем
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock);

        // --- Фильтры (каждый применяется только если параметр передан) ---
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(p => p.Brand == brand);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (minRating.HasValue)
            query = query.Where(p => p.AverageRating >= minRating.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));

        // --- Сортировка ---
        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "rating_desc" => query.OrderByDescending(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.Price) // price_desc по умолчанию
        };

        var total = await query.CountAsync(cancellationToken);

        // Skip/Take = пагинация: страница N, по pageSize элементов
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Brand,
                p.Price,
                p.OldPrice,
                // Скидка в % — как бейдж «-25%» в макете
                DiscountPercent = p.OldPrice != null && p.OldPrice > p.Price
                    ? (int?)Math.Round((p.OldPrice.Value - p.Price) / p.OldPrice.Value * 100)
                    : null,
                p.AverageRating,
                p.ReviewCount,
                p.IsBestSeller,
                p.Status,
                // Главное фото, иначе первое по SortOrder
                ImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.Url)
                    .FirstOrDefault()
                    ?? p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        });
    }

    /// <summary>
    /// Полная карточка товара для Product Page:
    /// галерея, specs, about, категория.
    /// GET /api/products/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Sku,
                p.Brand,
                p.Price,
                p.OldPrice,
                DiscountPercent = p.OldPrice != null && p.OldPrice > p.Price
                    ? (int?)Math.Round((p.OldPrice.Value - p.Price) / p.OldPrice.Value * 100)
                    : null,
                p.StockQuantity,
                p.Status,
                p.AverageRating,
                p.ReviewCount,
                p.IsBestSeller,
                Category = new { p.Category.Id, p.Category.Name, p.Category.Slug },
                Images = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new { i.Id, i.Url, i.IsPrimary, i.IsVideo, i.AltText }),
                Attributes = p.Attributes
                    .OrderBy(a => a.SortOrder)
                    .Select(a => new { a.Name, a.Value }),
                AboutItems = p.AboutItems
                    .OrderBy(a => a.SortOrder)
                    .Select(a => new { a.Title, a.Description })
            })
            .FirstOrDefaultAsync(cancellationToken);

        // 404, если товар не найден
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Slug == slug)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Sku,
                p.Brand,
                p.Price,
                p.OldPrice,
                p.StockQuantity,
                p.Status,
                p.AverageRating,
                p.ReviewCount,
                Category = new { p.Category.Id, p.Category.Name, p.Category.Slug }
            })
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    public record CreateProductRequest(
        string Name,
        string? Description,
        string? Sku,
        string? Brand,
        string? Slug,
        Guid CategoryId,
        decimal Price,
        decimal? OldPrice,
        int StockQuantity);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest body,
        [FromServices] IProductService products,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Name) || body.CategoryId == Guid.Empty)
            return BadRequest(new { error = "Name и CategoryId обязательны." });

        var sku = string.IsNullOrWhiteSpace(body.Sku)
            ? "SKU-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
            : body.Sku.Trim();

        if (await _db.Products.AnyAsync(p => p.Sku == sku, ct))
            return Conflict(new { error = "SKU занят." });

        var entity = new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = body.Name.Trim(),
            Description = body.Description?.Trim() ?? string.Empty,
            Sku = sku,
            Brand = body.Brand?.Trim() ?? "Du Soleil",
            CategoryId = body.CategoryId,
            Price = body.Price,
            OldPrice = body.OldPrice,
            StockQuantity = body.StockQuantity,
            Status = body.StockQuantity <= 0 ? ProductStatus.OutOfStock : ProductStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            Slug = body.Slug?.Trim() ?? string.Empty
        };

        await products.EnsureSlugAsync(entity, ct);
        if (!await products.IsSlugUniqueAsync(entity.Slug, null, ct))
            entity.Slug = SlugHelper.Unique(entity.Slug, s => _db.Products.Any(p => p.Slug == s));

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { entity.Id, entity.Slug });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, [FromServices] IProductService products, CancellationToken ct)
    {
        await products.SoftArchiveAsync(id, ct);
        return NoContent();
    }
}
