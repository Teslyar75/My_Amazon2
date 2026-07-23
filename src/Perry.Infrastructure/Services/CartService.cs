using Perry.Domain.Entities;
using Perry.Domain.Enums;
using Perry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Perry.Infrastructure.Services;

/// <summary>
/// Корзина с поддержкой гостя (SessionId) и пользователя (UserId).
/// Логика из homework CartService, адаптирована под сущность Cart.
/// </summary>
public interface ICartService
{
    Task<Cart> GetOrCreateCartAsync(Guid? userId, string? sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid? userId, string? sessionId, CancellationToken ct = default);
    Task AddAsync(Guid? userId, string? sessionId, Guid productId, int quantity = 1, CancellationToken ct = default);
    Task UpdateQuantityAsync(Guid? userId, string? sessionId, Guid productId, int quantity, CancellationToken ct = default);
    Task RemoveAsync(Guid? userId, string? sessionId, Guid productId, CancellationToken ct = default);
    Task ClearAsync(Guid? userId, string? sessionId, CancellationToken ct = default);
    Task MergeGuestToUserAsync(string sessionId, Guid userId, CancellationToken ct = default);
    Task<int> GetCountAsync(Guid? userId, string? sessionId, CancellationToken ct = default);
    Task<decimal> GetTotalAsync(Guid? userId, string? sessionId, CancellationToken ct = default);
}

public class CartService : ICartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db) => _db = db;

    public async Task<Cart> GetOrCreateCartAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        Cart? cart = null;

        if (userId.HasValue)
        {
            cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        }
        else if (!string.IsNullOrWhiteSpace(sessionId))
        {
            cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null, ct);
        }

        if (cart is not null)
            return cart;

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = userId.HasValue ? null : sessionId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(userId, sessionId, ct);
        if (cart is null)
            return [];

        return await _db.CartItems
            .AsNoTracking()
            .Include(i => i.Product)
            .ThenInclude(p => p.Images)
            .Where(i => i.CartId == cart.Id)
            .OrderByDescending(i => i.AddedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Guid? userId, string? sessionId, Guid productId, int quantity = 1, CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть больше нуля.");

        var product = await _db.Products.FirstOrDefaultAsync(
            p => p.Id == productId && p.Status != ProductStatus.Archived && p.Status != ProductStatus.Draft, ct)
            ?? throw new ArgumentException("Товар не найден.");

        if (product.Status == ProductStatus.OutOfStock || product.StockQuantity <= 0)
            throw new InvalidOperationException("Товара нет в наличии.");

        var cart = await GetOrCreateCartAsync(userId, sessionId, ct);
        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == productId, ct);
        var newQty = (item?.Quantity ?? 0) + quantity;

        if (newQty > product.StockQuantity)
            throw new InvalidOperationException($"Недостаточно на складе. Доступно: {product.StockQuantity}.");

        if (item is null)
        {
            _db.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                AddedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            item.Quantity = newQty;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateQuantityAsync(Guid? userId, string? sessionId, Guid productId, int quantity, CancellationToken ct = default)
    {
        if (quantity < 0)
            throw new ArgumentException("Количество не может быть отрицательным.");

        var cart = await FindCartAsync(userId, sessionId, ct)
            ?? throw new InvalidOperationException("Корзина не найдена.");

        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == productId, ct);
        if (item is null)
            return;

        if (quantity == 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
                ?? throw new ArgumentException("Товар не найден.");

            if (quantity > product.StockQuantity)
                throw new InvalidOperationException($"Недостаточно на складе. Доступно: {product.StockQuantity}.");

            item.Quantity = quantity;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Guid? userId, string? sessionId, Guid productId, CancellationToken ct = default)
    {
        await UpdateQuantityAsync(userId, sessionId, productId, 0, ct);
    }

    public async Task ClearAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(userId, sessionId, ct);
        if (cart is null)
            return;

        var items = await _db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        _db.CartItems.RemoveRange(items);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MergeGuestToUserAsync(string sessionId, Guid userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        var guest = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null, ct);

        if (guest is null || guest.Items.Count == 0)
            return;

        var userCart = await GetOrCreateCartAsync(userId, null, ct);

        foreach (var gi in guest.Items.ToList())
        {
            var existing = await _db.CartItems
                .FirstOrDefaultAsync(i => i.CartId == userCart.Id && i.ProductId == gi.ProductId, ct);

            if (existing is null)
            {
                gi.CartId = userCart.Id;
            }
            else
            {
                existing.Quantity += gi.Quantity;
                _db.CartItems.Remove(gi);
            }
        }

        _db.Carts.Remove(guest);
        userCart.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetCountAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(userId, sessionId, ct);
        if (cart is null)
            return 0;

        return await _db.CartItems.Where(i => i.CartId == cart.Id).SumAsync(i => (int?)i.Quantity ?? 0, ct);
    }

    public async Task<decimal> GetTotalAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(userId, sessionId, ct);
        if (cart is null)
            return 0;

        return await _db.CartItems
            .Where(i => i.CartId == cart.Id)
            .Include(i => i.Product)
            .SumAsync(i => i.Quantity * i.Product.Price, ct);
    }

    private async Task<Cart?> FindCartAsync(Guid? userId, string? sessionId, CancellationToken ct)
    {
        if (userId.HasValue)
            return await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (!string.IsNullOrWhiteSpace(sessionId))
            return await _db.Carts.FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null, ct);

        return null;
    }
}
