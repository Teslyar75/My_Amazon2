using System.Security.Claims;
using DuSoleil.Infrastructure.Services;

namespace DuSoleil.Web.Extensions;

/// <summary>Хелперы: user id из claims + guest session id для корзины.</summary>
public static class HttpContextCartExtensions
{
    public const string GuestCookieName = "ds_cart_sid";

    public static Guid? GetUserId(this HttpContext http)
    {
        var raw = http.User.FindFirstValue("Id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static string GetOrCreateGuestSessionId(this HttpContext http)
    {
        if (http.Request.Cookies.TryGetValue(GuestCookieName, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
            return existing;

        var sid = Guid.NewGuid().ToString("N");
        http.Response.Cookies.Append(GuestCookieName, sid, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        return sid;
    }

    public static async Task<int> GetCartBadgeCountAsync(this HttpContext http, ICartService cart)
    {
        var userId = http.GetUserId();
        var sid = userId.HasValue ? null : http.Request.Cookies[GuestCookieName];
        return await cart.GetCountAsync(userId, sid);
    }
}
