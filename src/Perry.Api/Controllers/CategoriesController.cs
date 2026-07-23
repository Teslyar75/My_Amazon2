using Perry.Infrastructure.Persistence;
using Perry.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Perry.Api.Controllers;

/// <summary>
/// API категорий каталога.
/// Маршрут: /api/categories
/// Нужен для меню, breadcrumbs и фильтра «Category» в админке.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    // AppDbContext внедряется через DI (см. DependencyInjection.cs)
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Возвращает дерево: корневые категории + их прямые подкатегории.
    /// GET /api/categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        // AsNoTracking — только чтение, без отслеживания изменений (быстрее)
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.ParentCategoryId == null) // только корни
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.ImageUrl,
                SubCategories = c.SubCategories
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Slug,
                        s.ImageUrl
                    })
            })
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.AsNoTracking()
            .Where(c => c.Slug == slug)
            .Select(c => new { c.Id, c.Name, c.Slug, c.ImageUrl, c.IsActive, c.ParentCategoryId })
            .FirstOrDefaultAsync(cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    public record CategoryWriteRequest(string Name, string? Slug, Guid? ParentCategoryId, int SortOrder = 0);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CategoryWriteRequest body,
        [FromServices] ICategoryService categories,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest(new { error = "Name обязателен." });

        var slug = string.IsNullOrWhiteSpace(body.Slug)
            ? SlugHelper.FromName(body.Name)
            : body.Slug.Trim().ToLowerInvariant();
        slug = SlugHelper.Unique(slug, s => _db.Categories.Any(c => c.Slug == s));

        var entity = new Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Name = body.Name.Trim(),
            Slug = slug,
            ParentCategoryId = body.ParentCategoryId,
            SortOrder = body.SortOrder,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetBySlug), new { slug = entity.Slug }, new { entity.Id, entity.Slug });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, [FromServices] ICategoryService categories, CancellationToken ct)
    {
        await categories.SoftDeactivateAsync(id, ct);
        return NoContent();
    }
}
