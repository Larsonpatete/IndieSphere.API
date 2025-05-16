using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace IndieSphere.ApiService.Controllers;

public class AccountController(IMediator mediator, IConfiguration config, IHttpClientFactory clientFactory) : ApiControllerBase
{
    private readonly IMediator mediator = mediator;
    private readonly IConfiguration config = config;
    private readonly IHttpClientFactory clientFactory = clientFactory;
    private readonly string _clientId = config["OAuth:ClientId"];
    private readonly string _clientSecret = config["OAuth:ClientSecret"];
    private readonly string _authorizationEndpoint = config["OAuth:AuthorizationEndpoint"];
    private readonly string _tokenEndpoint = config["OAuth:TokenEndpoint"];
    private readonly string _scope = config["OAuth:Scope"];

    [HttpGet("authorize")]
    public IActionResult Authorize([FromQuery] string successRedirectUrl, [FromQuery] string failureRedirectUrl)
    {
        // Store redirect URLs in temporary storage (session, cache, etc.)
        // You may want to use TempData, Session, or a distributed cache depending on your setup
        HttpContext.Session.SetString("SuccessRedirectUrl", successRedirectUrl);
        HttpContext.Session.SetString("FailureRedirectUrl", failureRedirectUrl);

        // Generate a state parameter to prevent CSRF attacks
        string state = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("OAuthState", state);

        // Build the authorization URL
        var authorizationUrl = new UriBuilder(_authorizationEndpoint)
        {
            Query = $"client_id={_clientId}&response_type=code&scope={_scope}&state={state}&redirect_uri={Url.Action("Callback", "OAuth", null, Request.Scheme)}"
        };

        // Redirect to the authorization endpoint
        return Redirect(authorizationUrl.ToString());
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error)
    {
        // Retrieve redirect URLs from session
        string successRedirectUrl = HttpContext.Session.GetString("SuccessRedirectUrl") ?? "/";
        string failureRedirectUrl = HttpContext.Session.GetString("FailureRedirectUrl") ?? "/";

        // Verify the state parameter to prevent CSRF attacks
        string storedState = HttpContext.Session.GetString("OAuthState");
        if (string.IsNullOrEmpty(storedState) || state != storedState)
        {
            return Redirect($"{failureRedirectUrl}?error=invalid_state");
        }

        // Clear the state from session
        HttpContext.Session.Remove("OAuthState");

        // Check if there was an error returned from the authorization server
        if (!string.IsNullOrEmpty(error))
        {
            return Redirect($"{failureRedirectUrl}?error={error}");
        }

        // Exchange the authorization code for an access token
        try
        {
            var tokenResponse = await ExchangeCodeForTokensAsync(code);

            // Check if token exchange was successful
            if (tokenResponse.ContainsKey("access_token"))
            {
                // Success! Redirect to the success URL with the tokens
                string accessToken = tokenResponse["access_token"];
                string tokenType = tokenResponse["token_type"];
                string expiresIn = tokenResponse["expires_in"];
                string refreshToken = tokenResponse.ContainsKey("refresh_token") ? tokenResponse["refresh_token"] : null;

                // Store the tokens securely (e.g., in a session or encrypted cookie)
                // HttpContext.Session.SetString("AccessToken", accessToken);

                // Append token information to the redirect URL if needed
                // For production, consider using a safer approach than adding tokens to URLs
                var redirectUri = new UriBuilder(successRedirectUrl);
                var query = System.Web.HttpUtility.ParseQueryString(redirectUri.Query);
                query["auth_success"] = "true";
                redirectUri.Query = query.ToString();

                return Redirect(redirectUri.ToString());
            }
            else
            {
                // Token exchange failed
                return Redirect($"{failureRedirectUrl}?error=token_exchange_failed");
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Token exchange error: {ex.Message}");
            return Redirect($"{failureRedirectUrl}?error=exception&message={Uri.EscapeDataString(ex.Message)}");
        }
    }
    private async Task<Dictionary<string, string>> ExchangeCodeForTokensAsync(string code)
    {
        var httpClient = clientFactory.CreateClient();

        // Prepare the token request
        var tokenRequestContent = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("redirect_uri", Url.Action("Callback", "OAuth", null, Request.Scheme))
            });

        // Send the token request
        var response = await httpClient.PostAsync(_tokenEndpoint, tokenRequestContent);
        response.EnsureSuccessStatusCode();

        // Parse the JSON response
        var responseContent = await response.Content.ReadAsStringAsync();
        using (JsonDocument document = JsonDocument.Parse(responseContent))
        {
            var root = document.RootElement;
            var result = new Dictionary<string, string>();

            foreach (var property in root.EnumerateObject())
            {
                result[property.Name] = property.Value.ToString();
            }

            return result;
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var httpClient = clientFactory.CreateClient();

            // Prepare the token refresh request
            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", request.RefreshToken),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                });

            // Send the token request
            var response = await httpClient.PostAsync(_tokenEndpoint, tokenRequestContent);
            response.EnsureSuccessStatusCode();

            // Return the JSON response directly
            var responseContent = await response.Content.ReadAsStringAsync();
            return Content(responseContent, "application/json");
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Token refresh error: {ex.Message}");
            return BadRequest(new { error = "token_refresh_failed", error_description = ex.Message });
        }
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}

