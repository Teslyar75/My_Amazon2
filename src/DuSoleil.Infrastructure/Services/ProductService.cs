using DuSoleil.Domain.Entities;
using DuSoleil.Domain.Enums;
using DuSoleil.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Infrastructure.Services;

/// <summary>Товары: slug, related, soft-archive (homework ProductService).</summary>
public interface IProductService
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetRelatedAsync(Guid productId, int count = 6, CancellationToken ct = default);
    Task SoftArchiveAsync(Guid productId, CancellationToken ct = default);
    Task RestoreAsync(Guid productId, CancellationToken ct = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task EnsureSlugAsync(Product product, CancellationToken ct = default);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status != ProductStatus.Archived, ct);

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(Guid productId, int count = 6, CancellationToken ct = default)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null)
            return [];

        var visible = _db.Products.AsNoTracking()
            .Include(p => p.Images)
            .Where(p => p.Id != productId
                && (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock));

        var same = await visible.Where(p => p.CategoryId == product.CategoryId).ToListAsync(ct);
        var other = await visible.Where(p => p.CategoryId != product.CategoryId).ToListAsync(ct);

        var rnd = Random.Shared;
        var takeSame = Math.Min(3, count);
        var takeOther = Math.Max(0, count - takeSame);

        return same.OrderBy(_ => rnd.Next()).Take(takeSame)
            .Concat(other.OrderBy(_ => rnd.Next()).Take(takeOther))
            .Take(count)
            .ToList();
    }

    public async Task SoftArchiveAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return;
        product.Status = ProductStatus.Archived;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return;
        product.Status = product.StockQuantity <= 0 ? ProductStatus.OutOfStock : ProductStatus.Active;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var q = _db.Products.Where(p => p.Slug == slug);
        if (excludeId.HasValue)
            q = q.Where(p => p.Id != excludeId);
        return !await q.AnyAsync(ct);
    }

    public async Task EnsureSlugAsync(Product product, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(product.Slug))
            return;

        var baseSlug = !string.IsNullOrWhiteSpace(product.Sku)
            ? SlugHelper.FromName(product.Sku)
            : SlugHelper.FromName(product.Name);

        product.Slug = SlugHelper.Unique(baseSlug, s =>
            _db.Products.Any(p => p.Slug == s && p.Id != product.Id));
        await Task.CompletedTask;
    }
}
