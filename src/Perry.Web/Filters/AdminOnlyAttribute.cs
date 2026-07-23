using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Perry.Web.Filters;

/// <summary>
/// Страница доступна только роли Admin (как проверка в ShopController.Admin homework).
/// </summary>
public class AdminOnlyAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var user = context.HttpContext.User;
        var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (!(user.Identity?.IsAuthenticated ?? false) || role != "Admin")
        {
            context.Result = new RedirectToPageResult("/Admin/Login");
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}
