using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace IndieSphere.ApiService.Controllers;

public class SpotifyController(IConfiguration config) : ApiControllerBase
{
    private readonly IConfiguration _configuration = config;

    // Initiate Spotify login
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action("UserInfo")
        }, "Spotify");
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        Console.WriteLine("do you ever hit");
        // Authentication is handled by middleware, just redirect to success
        return RedirectToAction("GetToken");
    }

    // Get access token
    [Authorize]
    [HttpGet("token")]
    public IActionResult GetToken()
    {
        var accessToken = HttpContext.GetTokenAsync("access_token").Result;
        return Content($"Access Token: {accessToken}");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true) // Check for null and authentication
            return Unauthorized();

        return Ok(new
        {
            Id = User.FindFirst("sub")?.Value,
            Name = User.FindFirst("urn:spotify:name")?.Value
        });
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

    private async Task<string> GetClientCredentialsToken()
    {
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(
            _configuration["Spotify:ClientId"],
            _configuration["Spotify:ClientSecret"]);
        var response = await new OAuthClient(config).RequestToken(request);
        return response.AccessToken;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        //var token = await GetClientCredentialsToken();
        //var spotify = new SpotifyClient(token);

        //var results = await spotify.Search.Item(new SearchRequest(
        //    SearchRequest.Types.All,
        //    query));

        //if (results?.Tracks?.Items == null)
        //{
        //    return Ok(Enumerable.Empty<Song>());
        //}

        //var songs = results.Tracks.Items.Select(t =>
        //{
        //    if (t == null) return null;

        //    // Get first artist
        //    var primaryArtist = t.Artists?.FirstOrDefault();
        //    var artistName = primaryArtist?.Name ?? string.Empty;

        //    // Get artist Spotify URL
        //    Uri artistUrl = null;
        //    if (primaryArtist?.ExternalUrls?.ContainsKey("spotify") == true)
        //    {
        //        Uri.TryCreate(primaryArtist.ExternalUrls["spotify"], UriKind.Absolute, out artistUrl);
        //    }

        //    // Get track Spotify URL
        //    Uri trackUrl = null;
        //    if (t.ExternalUrls?.ContainsKey("spotify") == true)
        //    {
        //        Uri.TryCreate(t.ExternalUrls["spotify"], UriKind.Absolute, out trackUrl);
        //    }

        //    // Get album image (try to get the medium size first, fall back to any available image)
        //    Uri albumImageUrl = null;
        //    if (t.Album?.Images?.Count > 0)
        //    {
        //        // Try to get medium (300px) image first, or the first available if not found
        //        var image = t.Album.Images.FirstOrDefault(i => i.Height == 300 || i.Width == 300)
        //                   ?? t.Album.Images.FirstOrDefault();

        //        if (image?.Url != null)
        //        {
        //            Uri.TryCreate(image.Url, UriKind.Absolute, out albumImageUrl);
        //        }
        //    }
        //    var albumName = t.Album?.Name ?? string.Empty;

        //    return new Song(
        //        t.Name ?? "Unknown Track",
        //        artistName,
        //        artistUrl,
        //        trackUrl,
        //        albumImageUrl,
        //        albumName
        //    );
        //}).Where(song => song != null);

        //return Ok(songs);
        return Ok();
    }
}