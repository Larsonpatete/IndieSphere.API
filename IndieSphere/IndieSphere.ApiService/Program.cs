using Azure.Identity;
using IndieSphere.Application;
using IndieSphere.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;



var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("IndieSphereDb");

builder.Services
    .AddProblemDetails()
    .AddOpenApi()
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(connectionString)
    .AddControllers()
    ;

// Add JWT validation for Google tokens
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.Authority = "https://accounts.google.com";
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidIssuer = "accounts.google.com",
//            ValidAudience = builder.Configuration["Authentication:Google:ClientId"],
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true
//        };
//    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Spotify";
})
.AddCookie()
.AddOAuth("Spotify", options =>
{
    // Update the assignment to handle potential null values using null-coalescing operator or throw an exception if null.
    options.ClientId = builder.Configuration["Spotify:ClientId"] ?? throw new InvalidOperationException("Spotify:ClientId is not configured.");
    options.ClientSecret = builder.Configuration["Spotify:ClientSecret"] ?? throw new InvalidOperationException("Spotify:ClientSecret is not configured.");
    options.CallbackPath = new PathString("/api/spotify/callback");

    // Spotify OAuth endpoints
    options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    options.TokenEndpoint = "https://accounts.spotify.com/api/token";
    options.UserInformationEndpoint = "https://api.spotify.com/v1/me";
    
    // Set the full redirect URI
    var redirectUri = builder.Configuration["Spotify:RedirectUri"];
    if (!string.IsNullOrEmpty(redirectUri))
    {
        options.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                // Build the authorization URL manually with all required parameters
                var parameters = new Dictionary<string, string>
                {
                    ["client_id"] = options.ClientId,
                    ["response_type"] = "code",
                    ["redirect_uri"] = redirectUri,
                    ["scope"] = string.Join(" ", options.Scope),
                    ["state"] = context.Properties.Items[".xsrf"]
                };
                
                // Add any additional parameters from original request
                foreach (var item in context.Properties.Items)
                {
                    if (item.Key.StartsWith("oauth.") && 
                        !parameters.ContainsKey(item.Key.Substring("oauth.".Length)))
                    {
                        parameters[item.Key.Substring("oauth.".Length)] = item.Value;
                    }
                }
                
                // Build the query string
                var queryString = string.Join("&", parameters.Select(kvp => 
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                
                // Redirect to the authorization endpoint with our custom URL
                context.Response.Redirect($"{options.AuthorizationEndpoint}?{queryString}");
                return Task.CompletedTask;
            }
        };
    }

    // Add scopes
    options.Scope.Add("user-top-read");
    options.Scope.Add("user-read-recently-played");
    options.Scope.Add("user-read-private");
    options.Scope.Add("user-read-email");

    // Save tokens
    options.SaveTokens = true;

    // Map Spotify claims
    options.ClaimActions.MapJsonKey("sub", "id");
    options.ClaimActions.MapJsonKey("urn:spotify:name", "display_name");
});


builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorUI", policy =>
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

app.UseCors("BlazorUI");

app.Run();
