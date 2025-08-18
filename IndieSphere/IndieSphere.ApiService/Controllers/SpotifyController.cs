using IndieSphere.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndieSphere.ApiService.Controllers;

public class SpotifyController(IConfiguration config, ITokenService tokenService) : ApiControllerBase
{
    private readonly IConfiguration _configuration = config;
    private readonly ITokenService _tokenService = tokenService;

    [HttpGet("login")]
    public IActionResult Login()
    {
        // Don't redirect to your callback - redirect to a frontend page
        // This lets the middleware complete the flow without your callback
        var properties = new AuthenticationProperties
        {
            RedirectUri = "http://localhost:3000/"
        };

        return Challenge(properties, "Spotify");
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        Console.WriteLine("Callback hit!"); // For debugging

        try
        {
            var result = await HttpContext.AuthenticateAsync("Spotify");
            if (!result.Succeeded)
            {
                Console.WriteLine($"Authentication failed: {result.Failure?.Message}");
                return Redirect("http://localhost:3000/login-failed");
            }

            // Get claims from Spotify OAuth
            var claims = result.Principal.Claims.ToList();

            // Create JWT token for our app
            var jwtToken = _tokenService.CreateToken(claims);

            return Redirect($"http://localhost:3000/login-success?token={jwtToken}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in callback: {ex}");
            return Redirect("http://localhost:3000/login-failed");
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("http://localhost:3000/");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        // The user's ID is now in the claims from the JWT.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // You can return the claims directly or use them to fetch more data.
        var userProfile = new
        {
            Id = userId,
            DisplayName = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email)
        };

        return Ok(userProfile);
    }
}