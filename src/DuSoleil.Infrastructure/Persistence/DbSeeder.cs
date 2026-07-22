using DuSoleil.Domain.Entities;
using DuSoleil.Domain.Enums;
using DuSoleil.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DuSoleil.Infrastructure.Persistence;

/// <summary>
/// Заполняет БД демо-данными при первом запуске (если таблица Products пустая).
/// Нужно для тестирования главной и карточки товара.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
        await EnsureProductSlugsAsync(db);

        if (await db.Products.AnyAsync())
            return;

        // --- Категории (дерево как в макете) ---
        var electronics = Cat("Electronics", "electronics", 1);
        var streaming = Cat("Streaming devices", "streaming-devices", 1, electronics.Id);
        var fashion = Cat("Fashion", "fashion", 2);
        var women = Cat("Women's fashion", "womens-fashion", 1, fashion.Id);
        var casual = Cat("Casual Women's Clothing", "casual-womens-clothing", 1, women.Id);
        var tops = Cat("Tops, Tees & Blouses", "tops-tees-blouses", 1, casual.Id);
        var pcs = Cat("PCs & Accessories", "pcs-accessories", 2, electronics.Id);

        db.Categories.AddRange(electronics, streaming, fashion, women, casual, tops, pcs);
        await db.SaveChangesAsync();

        // --- Товары ---
        var roku = Product(
            name: "Roku Express 4K+ | Roku Streaming Device 4K/HDR, Roku Voice Remote, Free & Live TV",
            sku: "3941R2",
            brand: "Roku",
            categoryId: streaming.Id,
            price: 29.00m,
            oldPrice: 39.00m,
            stock: 48,
            rating: 4.0m,
            reviews: 8620,
            bestSeller: true,
            description: "Brilliant 4K picture quality with HDR. Seamless streaming to your TV. Voice search & control with the Roku Voice Remote.");

        var tee1 = Product(
            name: "PUMIEY Women's Long Sleeve T-Shirt Soft Lightweight Tee",
            sku: "PUM-LS-001",
            brand: "PUMIEY",
            categoryId: tops.Id,
            price: 19.99m,
            oldPrice: 32.99m,
            stock: 120,
            rating: 4.3m,
            reviews: 448,
            bestSeller: true,
            description: "Soft stretch fabric, relaxed fit. Everyday essential for casual looks.");

        var tee2 = Product(
            name: "Abardsion Women's Classic Crew Neck Tee",
            sku: "ABR-CR-014",
            brand: "Abardsion",
            categoryId: tops.Id,
            price: 14.50m,
            oldPrice: 24.00m,
            stock: 80,
            rating: 4.1m,
            reviews: 312,
            bestSeller: false,
            description: "Breathable cotton blend crew neck tee for daily wear.");

        var dress = Product(
            name: "Dokotoo Womens Dresses 2024 Summer Casual Midi Dress",
            sku: "DKT-DR-2024",
            brand: "Dokotoo",
            categoryId: casual.Id,
            price: 24.99m,
            oldPrice: 32.99m,
            stock: 35,
            rating: 4.5m,
            reviews: 1204,
            bestSeller: true,
            description: "Flowy midi dress with soft fabric. Perfect for summer days.");

        var shoes = Product(
            name: "WateLves Water Shoes Mens Quick-Dry Aqua Socks",
            sku: "WTL-WS-088",
            brand: "WateLves",
            categoryId: fashion.Id,
            price: 16.99m,
            oldPrice: null,
            stock: 0,
            rating: 4.2m,
            reviews: 567,
            bestSeller: false,
            description: "Lightweight water shoes with quick-dry mesh. Ideal for beach and pool.",
            status: ProductStatus.OutOfStock);

        var headset = Product(
            name: "Essentials Wireless On-Ear Headphones",
            sku: "ESS-HP-220",
            brand: "Essentials",
            categoryId: pcs.Id,
            price: 7.40m,
            oldPrice: 14.20m,
            stock: 200,
            rating: 4.3m,
            reviews: 1547,
            bestSeller: true,
            description: "Comfortable on-ear headphones with wireless Bluetooth connectivity.");

        var keyboard = Product(
            name: "Deals in PCs Compact Mechanical Keyboard",
            sku: "PC-KB-441",
            brand: "KeyPro",
            categoryId: pcs.Id,
            price: 45.00m,
            oldPrice: 59.00m,
            stock: 60,
            rating: 4.6m,
            reviews: 890,
            bestSeller: true,
            description: "Compact mechanical keyboard with RGB backlight for work and gaming.");

        var hat = Product(
            name: "Lack of Color Women's Ventura Hat",
            sku: "LOC-VT-003",
            brand: "Lack of Color",
            categoryId: women.Id,
            price: 89.00m,
            oldPrice: 110.00m,
            stock: 22,
            rating: 4.4m,
            reviews: 156,
            bestSeller: false,
            description: "Stylish wide-brim hat for sunny days.");

        db.Products.AddRange(roku, tee1, tee2, dress, shoes, headset, keyboard, hat);
        await db.SaveChangesAsync();

        // --- Картинки (placeholder-сервис, стабильные URL) ---
        AddImages(db, roku.Id, "roku");
        AddImages(db, tee1.Id, "tshirt");
        AddImages(db, tee2.Id, "tee");
        AddImages(db, dress.Id, "dress");
        AddImages(db, shoes.Id, "shoes");
        AddImages(db, headset.Id, "headphones");
        AddImages(db, keyboard.Id, "keyboard");
        AddImages(db, hat.Id, "hat");

        // --- Характеристики Roku (как в макете Product Page) ---
        db.ProductAttributes.AddRange(
            Attr(roku.Id, "Brand", "Roku", 1),
            Attr(roku.Id, "Color", "Black", 2),
            Attr(roku.Id, "Item weight", "1.6 ounces", 3),
            Attr(roku.Id, "Product dimensions", "3 x 1.5 x 0.83 inches", 4),
            Attr(roku.Id, "Batteries", "2 AAA batteries required", 5),
            Attr(roku.Id, "Item model number", "3941R2", 6),
            Attr(tee1.Id, "Brand", "PUMIEY", 1, true),
            Attr(tee1.Id, "Fabric type", "Polyamide, Elastane", 2, true),
            Attr(tee1.Id, "Color", "Black", 3, true),
            Attr(tee1.Id, "Size", "M", 4, true));

        db.ProductAboutItems.AddRange(
            About(roku.Id, "Brilliant 4K picture quality", "Enjoy sharp 4K HDR streaming on compatible TVs.", 1),
            About(roku.Id, "Seamless streaming", "Launch your favorite channels in seconds from the home screen.", 2),
            About(roku.Id, "Voice search & control", "Use the Roku Voice Remote to find shows hands-free.", 3),
            About(roku.Id, "Free & Live TV", "Access free live channels and popular streaming apps.", 4));

        // --- Отзывы ---
        var review1 = new ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = roku.Id,
            AuthorName = "Alex M.",
            Rating = 5,
            Title = "Easy to use",
            Body = "Set up in minutes. Picture looks great in 4K. Remote voice search works well.",
            IsApproved = true,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-12)
        };
        var review2 = new ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = roku.Id,
            AuthorName = "Jordan K.",
            Rating = 4,
            Title = "Great value",
            Body = "Does everything I need. Wish the remote had a headphone jack, but still recommend.",
            IsApproved = true,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5)
        };

        db.ProductReviews.AddRange(review1, review2);
        db.ProductReviewTags.AddRange(
            new ProductReviewTag { Id = Guid.NewGuid(), ReviewId = review1.Id, Name = "easy to use" },
            new ProductReviewTag { Id = Guid.NewGuid(), ReviewId = review1.Id, Name = "remote control" },
            new ProductReviewTag { Id = Guid.NewGuid(), ReviewId = review2.Id, Name = "great value" });

        await db.SaveChangesAsync();
    }

    /// <summary>Backfill Slug для уже существующих товаров после миграции.</summary>
    private static async Task EnsureProductSlugsAsync(AppDbContext db)
    {
        var missing = await db.Products
            .Where(p => p.Slug == null || p.Slug == "")
            .ToListAsync();
        if (missing.Count == 0)
            return;

        var used = await db.Products
            .Where(p => p.Slug != null && p.Slug != "")
            .Select(p => p.Slug)
            .ToListAsync();
        var usedSet = used.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var p in missing)
        {
            var baseSlug = SlugHelper.FromName(string.IsNullOrWhiteSpace(p.Sku) ? p.Name : p.Sku);
            p.Slug = SlugHelper.Unique(baseSlug, s => usedSet.Contains(s));
            usedSet.Add(p.Slug);
        }

        await db.SaveChangesAsync();
    }

    private static Category Cat(string name, string slug, int order, Guid? parentId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        SortOrder = order,
        IsActive = true,
        ParentCategoryId = parentId,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static Product Product(
        string name,
        string sku,
        string brand,
        Guid categoryId,
        decimal price,
        decimal? oldPrice,
        int stock,
        decimal rating,
        int reviews,
        bool bestSeller,
        string description,
        ProductStatus status = ProductStatus.Active) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Sku = sku,
        Slug = SlugHelper.FromName(sku),
        Brand = brand,
        CategoryId = categoryId,
        Price = price,
        OldPrice = oldPrice,
        StockQuantity = stock,
        Status = stock <= 0 ? ProductStatus.OutOfStock : status,
        AverageRating = rating,
        ReviewCount = reviews,
        IsBestSeller = bestSeller,
        Description = description,
        CreatedAtUtc = DateTime.UtcNow
    };

    private static void AddImages(AppDbContext db, Guid productId, string seed)
    {
        for (var i = 0; i < 4; i++)
        {
            db.ProductImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Url = $"https://picsum.photos/seed/{seed}{i}/640/640",
                SortOrder = i,
                IsPrimary = i == 0,
                IsVideo = false,
                AltText = seed
            });
        }
    }

    private static ProductAttribute Attr(Guid productId, string name, string value, int order, bool filterable = false) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        Name = name,
        Value = value,
        SortOrder = order,
        IsFilterable = filterable
    };

    private static ProductAboutItem About(Guid productId, string title, string description, int order) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        Title = title,
        Description = description,
        SortOrder = order
    };
}
