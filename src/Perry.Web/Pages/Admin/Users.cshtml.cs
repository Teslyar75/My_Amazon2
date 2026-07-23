using Perry.Infrastructure.Services;
using Perry.Web.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Perry.Web.Pages.Admin;

/// <summary>Список пользователей — Figma Admin User + homework Users.</summary>
[AdminOnly]
public class UsersModel : PageModel
{
    private readonly IUserService _users;

    public UsersModel(IUserService users) => _users = users;

    public IReadOnlyList<UserRow> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var list = await _users.GetAllAsync(ct);
        Rows = list.Select(u => new UserRow
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Login = u.Accesses.FirstOrDefault()?.Login ?? "—",
            Role = u.Accesses.FirstOrDefault()?.RoleId ?? "—",
            RegisteredAtUtc = u.RegisteredAtUtc
        }).ToList();
    }

    public class UserRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime RegisteredAtUtc { get; set; }
    }
}
