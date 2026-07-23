using Perry.Domain.Enums;
using Perry.Infrastructure.Services;
using Perry.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Perry.Web.Pages.Admin;

/// <summary>Список заказов — Figma Admin Order + homework Orders.</summary>
[AdminOnly]
public class OrdersModel : PageModel
{
    private readonly IOrderService _orders;

    public OrdersModel(IOrderService orders) => _orders = orders;

    public IReadOnlyList<Domain.Entities.Order> Orders { get; private set; } = [];
    public string? Message { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Orders = await _orders.GetAllAsync(ct);
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id, OrderStatus status, CancellationToken ct)
    {
        await _orders.UpdateStatusAsync(id, status, ct);
        Message = "Статус заказа обновлён.";
        Orders = await _orders.GetAllAsync(ct);
        return Page();
    }
}
