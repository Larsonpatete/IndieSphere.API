using Azure.Identity;
using IndieSphere.Application;
using IndieSphere.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("IndieSphereDb");

builder.Services
    .AddProblemDetails()
    .AddOpenApi()
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(connectionString)
    .AddControllers();

builder.Services.AddAuthentication(options =>
{
    // The default scheme for signing in and out is cookies.
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // When a user needs to log in, they will be challenged with the Spotify scheme.
    options.DefaultChallengeScheme = "Spotify";
})
.AddCookie(options =>
{
    // Your existing settings
    options.LoginPath = "/api/spotify/login";

    // Add these cross-domain cookie settings
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
})
.AddOAuth("Spotify", options =>
{
    options.ClientId = builder.Configuration["Spotify:ClientId"];
    options.ClientSecret = builder.Configuration["Spotify:ClientSecret"];
    options.CallbackPath = new PathString("/api/spotify/callback");

    // Spotify's OAuth 2.0 endpoints.
    options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    options.TokenEndpoint = "https://accounts.spotify.com/api/token";
    options.UserInformationEndpoint = "https://api.spotify.com/v1/me";

    // Define the scopes your application needs.
    options.Scope.Add("user-read-private");
    options.Scope.Add("user-read-email");

    // This tells the handler to save the access_token and refresh_token from Spotify.
    options.SaveTokens = true;

    // Map user data from Spotify to claims in the user's identity.
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey("urn:spotify:profile", "external_urls:spotify");
});



builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactUI", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"]; // TODO: add KeyVault URL to configuration
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl ?? "https://indiesphere-kv.vault.azure.net/"),
        new DefaultAzureCredential());
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        // Handle preflight requests for all auth endpoints
        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.StatusCode = 204;
        return;
    }

    await next();
});

app.UseCors("ReactUI");

app.Run();
