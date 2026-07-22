using DuSoleil.Domain.Entities;
using DuSoleil.Infrastructure.Persistence;
using DuSoleil.Infrastructure.Services;
using DuSoleil.Web.Extensions;
using DuSoleil.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Web.Pages.Account;

/// <summary>Профиль покупателя — homework User/Profile (view + edit + soft-delete).</summary>
public class ProfileModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IOrderService _orders;
    private readonly IUserService _users;

    public ProfileModel(AppDbContext db, IOrderService orders, IUserService users)
    {
        _db = db;
        _orders = orders;
        _users = users;
    }

    public User? Account { get; private set; }
    public string? LoginName { get; private set; }
    public string? RoleId { get; private set; }
    public IReadOnlyList<Order> RecentOrders { get; private set; } = [];
    public int TotalOrders { get; private set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public string EditName { get; set; } = string.Empty;

    [BindProperty]
    public string EditEmail { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        return await LoadAsync(ct) ?? Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = "/Account/Profile" });

        try
        {
            await _users.UpdateAsync(userId.Value, EditName, EditEmail, ct);
            Message = "Профиль обновлён.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return await LoadAsync(ct) ?? Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
            return RedirectToPage("/Account/Login");

        try
        {
            await _users.SoftDeleteAsync(userId.Value, ct);
            HttpContext.Session.Remove(AuthSessionMiddleware.SessionKey);
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return await LoadAsync(ct) ?? Page();
        }
    }

    private async Task<IActionResult?> LoadAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = "/Account/Profile" });

        Account = await _users.GetByIdAsync(userId.Value, ct);
        if (Account is null)
            return NotFound();

        var access = await _db.UserAccesses.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);
        LoginName = access?.Login;
        RoleId = access?.RoleId;
        EditName = Account.Name;
        EditEmail = Account.Email;

        var orders = await _orders.GetUserOrdersAsync(userId.Value, ct);
        TotalOrders = orders.Count;
        RecentOrders = orders.Take(5).ToList();
        return null;
    }
}
