using DuSoleil.Domain.Entities;
using DuSoleil.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Infrastructure.Services;

/// <summary>Категории: soft-deactivate (homework ProductGroupService).</summary>
public interface ICategoryService
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task SoftDeactivateAsync(Guid categoryId, CancellationToken ct = default);
    Task ActivateAsync(Guid categoryId, CancellationToken ct = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db) => _db = db;

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, ct);

    public async Task SoftDeactivateAsync(Guid categoryId, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (cat is null) return;
        cat.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid categoryId, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (cat is null) return;
        cat.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var q = _db.Categories.Where(c => c.Slug == slug);
        if (excludeId.HasValue)
            q = q.Where(c => c.Id != excludeId);
        return !await q.AnyAsync(ct);
    }
}
