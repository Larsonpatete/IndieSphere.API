using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IndieSphere.ApiService.Middleware;

/// <summary>
/// This middleware ensures that for any request where the user might be authenticated via a cookie,
/// the full user principal and authentication properties (including tokens) are loaded.
/// This makes HttpContext.User.Identity.IsAuthenticated and HttpContext.GetTokenAsync
/// work reliably in all services, even for endpoints that do not have an [Authorize] attribute.
/// </summary>
public class UserAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // If a user appears to be authenticated (e.g., a cookie exists),
        // we will now force the full authentication to run. This will load the tokens
        // and correctly set User.Identity.IsAuthenticated.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // Continue to the next middleware in the pipeline.
        await next(context);
    }
}
