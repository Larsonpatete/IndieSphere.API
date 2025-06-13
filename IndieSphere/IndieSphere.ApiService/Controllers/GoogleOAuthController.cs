using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

[Authorize]
public class GoogleOAuthController : ApiControllerBase
{
    //[HttpGet("me")]
    //public IActionResult Callback()
    //{
    //    // Here you would typically create a JWT or session token
    //    // For this barebones example, we'll just return user info
    //    var userInfo = news
    //    {
    //        User.Identity.Name,
    //        Claims = User.Claims.Select(c => new { c.Type, c.Value })
    //    };

    //    return Ok(userInfo);
    //}
}
