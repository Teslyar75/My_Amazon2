using DuSoleil.Domain.Entities;
using DuSoleil.Infrastructure.Auth;
using DuSoleil.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DuSoleil.Web.Pages.Account;

/// <summary>Регистрация покупателя (роль Guest) — homework SignUp.</summary>
public class RegisterModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IKdfService _kdf;

    public RegisterModel(AppDbContext db, IKdfService kdf)
    {
        _db = db;
        _kdf = kdf;
    }

    [BindProperty] public string Name { get; set; } = string.Empty;
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Login { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Login)
            || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
        {
            Error = "Заполните все поля.";
            return Page();
        }

        if (await _db.UserAccesses.AnyAsync(a => a.Login == Login, ct))
        {
            Error = "Такой логин уже занят.";
            return Page();
        }

        var salt = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = Name.Trim(),
            Email = Email.Trim(),
            RegisteredAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        _db.UserAccesses.Add(new UserAccess
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = "Guest",
            Login = Login.Trim(),
            Salt = salt,
            Dk = _kdf.Dk(Password, salt)
        });
        await _db.SaveChangesAsync(ct);
        return RedirectToPage("/Account/Login");
    }
}
