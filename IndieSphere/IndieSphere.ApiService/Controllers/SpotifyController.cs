using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace IndieSphere.ApiService.Controllers;

public class SpotifyController(IConfiguration config) : ApiControllerBase
{
    private readonly IConfiguration _configuration = config;

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Spotify");
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        return Redirect("http://localhost:3000/login-success"); // change in prod
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
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("Access token not found.");
        }

        var spotify = new SpotifyClient(accessToken);

        try
        {
            var userProfile = await spotify.UserProfile.Current();
            
            return Ok(userProfile);
        }
        catch (APIException ex)
        {
            // Handle cases where the token might be expired or invalid.
            return StatusCode((int)ex.Response.StatusCode, ex.Message);
        }
    }

    // Your existing recommendations endpoint
    [HttpGet("recommendations")]
    [Authorize] // Requires authentication
    public async Task<IActionResult> GetRecs(string artistQuery)
    {
        var token = await HttpContext.GetTokenAsync("access_token");
        var spotify = new SpotifyClient(token);

        // Example recommendation logic
        var search = await spotify.Search.Item(new SearchRequest(
            SearchRequest.Types.Artist,
            artistQuery
        ));

        var unknownArtists = search.Artists.Items
            .Where(a => a.Popularity < 50)
            .Select(a => a.Name)
            .ToList();

        return Ok(unknownArtists);
    }
}