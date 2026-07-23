using System.Text.Json;
using Perry.Domain.Entities;
using Perry.Domain.Enums;
using Perry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Perry.Infrastructure.Services;

/// <summary>
/// История просмотров товаров (homework ViewedProductsService).
/// Хранит до 10 Id в Session; на витрине показывает последние N.
/// </summary>
public interface IViewedProductsService
{
    void AddViewedProduct(Guid productId);
    Task<IReadOnlyList<Product>> GetViewedProductsAsync(int maxItems = 8, CancellationToken ct = default);
    void ClearViewedProducts();
}

public class ViewedProductsService : IViewedProductsService
{
    public const string SessionKey = "ViewedProducts";
    private const int MaxStored = 10;

    private readonly IHttpContextAccessor _http;
    private readonly AppDbContext _db;

    public ViewedProductsService(IHttpContextAccessor http, AppDbContext db)
    {
        _http = http;
        _db = db;
    }

    public void AddViewedProduct(Guid productId)
    {
        if (productId == Guid.Empty)
            return;

        var session = _http.HttpContext?.Session;
        if (session is null)
            return;

        var ids = ReadIds(session);
        ids.Remove(productId);
        ids.Insert(0, productId);
        if (ids.Count > MaxStored)
            ids.RemoveRange(MaxStored, ids.Count - MaxStored);

        session.SetString(SessionKey, JsonSerializer.Serialize(ids));
    }

    public async Task<IReadOnlyList<Product>> GetViewedProductsAsync(int maxItems = 8, CancellationToken ct = default)
    {
        var session = _http.HttpContext?.Session;
        if (session is null)
            return [];

        var ids = ReadIds(session).Take(maxItems).ToList();
        if (ids.Count == 0)
            return [];

        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Where(p => ids.Contains(p.Id)
                && (p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock))
            .ToListAsync(ct);

        return products
            .OrderBy(p => ids.IndexOf(p.Id))
            .ToList();
    }

    public void ClearViewedProducts()
    {
        _http.HttpContext?.Session.Remove(SessionKey);
    }

    private static List<Guid> ReadIds(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
