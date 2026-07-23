using System.Text.Json;
using Perry.Infrastructure.Auth;
using Perry.Infrastructure.Persistence;
using Perry.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Perry.Web.Pages.Admin;

/// <summary>Вход в админку (Login=Admin, Password=Admin — как в homework).</summary>
public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IKdfService _kdf;

    public LoginModel(AppDbContext db, IKdfService kdf)
    {
        _db = db;
        _kdf = kdf;
    }

    [BindProperty]
    public string Login { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? Error { get; set; }

    public IActionResult OnGet()
    {
        if (User.IsInRole("Admin"))
            return RedirectToPage("/Admin/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var access = await _db.UserAccesses
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Login == Login, cancellationToken);

        if (access is null || access.User.DeletedAtUtc != null)
        {
            Error = "Неверный логин или пароль.";
            return Page();
        }

        var dk = _kdf.Dk(Password, access.Salt);
        if (!string.Equals(dk, access.Dk, StringComparison.OrdinalIgnoreCase))
        {
            Error = "Неверный логин или пароль.";
            return Page();
        }

        var dto = new UserAccessSessionDto
        {
            UserId = access.UserId,
            UserName = access.User.Name,
            Email = access.User.Email,
            Login = access.Login,
            RoleId = access.RoleId
        };

        HttpContext.Session.SetString(
            AuthSessionMiddleware.SessionKey,
            JsonSerializer.Serialize(dto));

        return RedirectToPage("/Admin/Index");
    }
}
