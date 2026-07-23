using Perry.Domain.Entities;
using Perry.Domain.Enums;
using Perry.Infrastructure.Persistence;
using Perry.Infrastructure.Services;
using Perry.Infrastructure.Storage;
using Perry.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Perry.Web.Pages.Admin;

/// <summary>Редактирование товара — homework Shop/AdminEdit под Figma Admin Product.</summary>
[AdminOnly]
public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;

    public EditModel(AppDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public ProductEditForm Form { get; set; } = new();

    public List<Category> Categories { get; private set; } = [];
    public string? Message { get; set; }
    public string? Error { get; set; }
    public string? CurrentImageUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == Id, ct);

        if (product is null)
            return NotFound();

        Form = new ProductEditForm
        {
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Slug = product.Slug,
            Brand = product.Brand,
            Price = product.Price,
            OldPrice = product.OldPrice,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            Status = product.Status,
            IsBestSeller = product.IsBestSeller
        };
        CurrentImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.Url
            ?? product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url;

        Categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == Id, ct);

        if (product is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(Form.Name) || Form.CategoryId == Guid.Empty)
        {
            Error = "Название и категория обязательны.";
            CurrentImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.Url;
            return Page();
        }

        product.Name = Form.Name.Trim();
        product.Description = Form.Description?.Trim() ?? string.Empty;
        product.Sku = Form.Sku?.Trim() ?? product.Sku;
        product.Slug = string.IsNullOrWhiteSpace(Form.Slug)
            ? SlugHelper.FromName(product.Name)
            : Form.Slug.Trim().ToLowerInvariant();
        product.Brand = Form.Brand?.Trim() ?? string.Empty;
        product.Price = Form.Price;
        product.OldPrice = Form.OldPrice;
        product.StockQuantity = Form.StockQuantity;
        product.CategoryId = Form.CategoryId;
        product.Status = Form.StockQuantity <= 0 && Form.Status == ProductStatus.Active
            ? ProductStatus.OutOfStock
            : Form.Status;
        product.IsBestSeller = Form.IsBestSeller;
        product.UpdatedAtUtc = DateTime.UtcNow;

        if (Form.Image is { Length: > 0 })
        {
            try
            {
                var file = _storage.Save(Form.Image);
                var url = "/uploads/" + file;
                var primary = product.Images.FirstOrDefault(i => i.IsPrimary);
                if (primary is null)
                {
                    _db.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Url = url,
                        IsPrimary = true,
                        SortOrder = 0
                    });
                }
                else
                {
                    primary.Url = url;
                }
            }
            catch (Exception ex)
            {
                Error = "Ошибка загрузки изображения: " + ex.Message;
                CurrentImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.Url;
                return Page();
            }
        }

        await _db.SaveChangesAsync(ct);
        Message = "Товар сохранён.";
        return RedirectToPage(new { id = Id });
    }

    public class ProductEditForm
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public string? Slug { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
        public ProductStatus Status { get; set; }
        public bool IsBestSeller { get; set; }
        public IFormFile? Image { get; set; }
    }
}
