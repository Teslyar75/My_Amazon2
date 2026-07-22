using System.Text.Json;
using DuSoleil.Infrastructure.Auth;
using DuSoleil.Infrastructure.Persistence;
using DuSoleil.Infrastructure.Services;
using DuSoleil.Web.Extensions;
using DuSoleil.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Web.Pages.Account;

/// <summary>Вход покупателя (и админа) — homework User SignIn + merge guest cart.</summary>
public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IKdfService _kdf;
    private readonly ICartService _cart;

    public LoginModel(AppDbContext db, IKdfService kdf, ICartService cart)
    {
        _db = db;
        _kdf = kdf;
        _cart = cart;
    }

    [BindProperty]
    public string Login { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? Error { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var access = await _db.UserAccesses
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Login == Login, ct);

        if (access is null || access.User.DeletedAtUtc != null
            || !string.Equals(_kdf.Dk(Password, access.Salt), access.Dk, StringComparison.OrdinalIgnoreCase))
        {
            Error = "Неверный логин или пароль.";
            return Page();
        }

        HttpContext.Session.SetString(AuthSessionMiddleware.SessionKey, JsonSerializer.Serialize(
            new UserAccessSessionDto
            {
                UserId = access.UserId,
                UserName = access.User.Name,
                Email = access.User.Email,
                Login = access.Login,
                RoleId = access.RoleId
            }));

        if (HttpContext.Request.Cookies.TryGetValue(HttpContextCartExtensions.GuestCookieName, out var sid)
            && !string.IsNullOrWhiteSpace(sid))
        {
            await _cart.MergeGuestToUserAsync(sid, access.UserId, ct);
        }

        if (access.RoleId == "Admin" && string.IsNullOrEmpty(ReturnUrl))
            return RedirectToPage("/Admin/Index");

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl);

        return RedirectToPage("/Index");
    }
}
