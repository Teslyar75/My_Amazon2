using DuSoleil.Domain.Entities;
using DuSoleil.Infrastructure.Services;
using DuSoleil.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DuSoleil.Web.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly IOrderService _orders;

    public DetailsModel(IOrderService orders) => _orders = orders;

    public Order? Order { get; private set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var result = await LoadOrderAsync(id, ct);
        return result ?? Page();
    }

    public async Task<IActionResult> OnPostRepeatAsync(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = $"/Orders/Details/{id}" });

        try
        {
            await _orders.RepeatOrderAsync(userId.Value, id, ct);
            return RedirectToPage("/Cart/Index");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            var result = await LoadOrderAsync(id, ct);
            return result ?? Page();
        }
    }

    private async Task<IActionResult?> LoadOrderAsync(Guid id, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && userId is null)
            return RedirectToPage("/Account/Login", new { returnUrl = $"/Orders/Details/{id}" });

        Order = await _orders.GetByIdAsync(id, isAdmin ? null : userId, ct);
        if (Order is null)
            return NotFound();

        if (!isAdmin && Order.UserId != userId)
            return Forbid();

        return null;
    }
}
