using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.Web.Infrastructure.Authentication;

[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login() => Challenge(new AuthenticationProperties
    {
        RedirectUri = "/"
    }, GoogleDefaults.AuthenticationScheme);

    [HttpGet("logout")]
    public IActionResult Logout() => SignOut(
        new AuthenticationProperties { RedirectUri = "/" },
        CookieAuthenticationDefaults.AuthenticationScheme);
}