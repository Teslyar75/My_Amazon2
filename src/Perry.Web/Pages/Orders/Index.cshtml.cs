using Perry.Domain.Entities;
using Perry.Infrastructure.Services;
using Perry.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Perry.Web.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderService _orders;

    public IndexModel(IOrderService orders) => _orders = orders;

    public IReadOnlyList<Order> Orders { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = "/Orders" });

        Orders = await _orders.GetUserOrdersAsync(userId.Value, ct);
        return Page();
    }
}
