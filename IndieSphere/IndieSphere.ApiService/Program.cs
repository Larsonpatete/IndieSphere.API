using Azure.Identity;
using IndieSphere.Application;
using IndieSphere.Application.Features.Users;
using IndieSphere.Infrastructure;
using IndieSphere.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    Console.WriteLine($"Key Vault URL: {keyVaultUrl}");

    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
    else
    {
        Console.WriteLine("Key Vault URL is null or empty");
    }
}

// Debug: List all configuration values
var allConfig = builder.Configuration.AsEnumerable();
foreach (var config in allConfig)
{
    Console.WriteLine($"{config.Key}: {config.Value}");
}

var connectionString = builder.Configuration.GetConnectionString("IndieSphereDb");
Console.WriteLine($"Connection string: {connectionString ?? "NULL"}");

// Ensure the connection string is not null or empty
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'IndieSphereDb' not found.");
}
builder.Services
    .AddProblemDetails()
    .AddOpenApi()
    .AddHttpContextAccessor()
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddControllers();

builder.Services.AddAuthentication(options =>
{
    // Default to JWT for authenticating API requests
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme = "Spotify";
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;  // For cross-domain requests
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddOAuth("Spotify", options =>
{
    options.ClientId = builder.Configuration["Spotify:ClientId"];
    options.ClientSecret = builder.Configuration["Spotify:ClientSecret"];
    options.CallbackPath = new PathString("/api/spotify/callback");

    options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    options.TokenEndpoint = "https://accounts.spotify.com/api/token";
    options.UserInformationEndpoint = "https://api.spotify.com/v1/me";

    options.Scope.Add("user-read-private");
    options.Scope.Add("user-read-email");
    options.Scope.Add("user-top-read");
    options.SaveTokens = true;

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // 1. Get the access token and fetch the user profile from Spotify.
            var accessToken = context.AccessToken;
            var refreshToken = context.RefreshToken;
            var expiresAt = context.ExpiresIn.HasValue
                ? DateTime.UtcNow.AddSeconds(context.ExpiresIn.Value.TotalSeconds)
                : DateTime.UtcNow.AddHours(1);

            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            // 2. Create or update the user in your database.
            using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var imageUrl = user.RootElement.TryGetProperty("images", out var images) && images.GetArrayLength() > 0
                ? images[0].GetProperty("url").GetString()
                : null;

            var spotifyId = user.RootElement.GetString("id");
            var displayName = user.RootElement.GetString("display_name");
            var email = user.RootElement.GetString("email");

            // 2. Create or update the user in your database.
            var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
            var userId = await mediator.Send(new CreateOrUpdateUserCommand(
                spotifyId,
                displayName,
                email,
                accessToken,
                refreshToken,
                expiresAt
            ));

            // 3. Create the claims for your application's identity.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()), // Use your app's user ID
                new(ClaimTypes.Name, displayName),
                new(ClaimTypes.Email, email),
                new("urn:spotify:profile_image_url", imageUrl ?? ""),
                new("urn:spotify:id", spotifyId) // Keep Spotify ID for reference if needed
            };

            // 4. Replace the context principal with your new claims identity.
            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
        },

        OnTicketReceived = context =>
        {
            // This event fires right after OnCreatingTicket.
            // The context.Principal now contains the claims we just created.

            // 1. Generate the JWT.
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            var jwtToken = tokenService.CreateToken(context.Principal.Claims);

            // 2. Build the redirect URI for your frontend.
            var frontendUrl = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Frontend:BaseUrl"];
            var redirectUrl = $"{frontendUrl}/login-success?token={jwtToken}";

            // 3. Redirect the user's browser to the frontend with the token.
            context.Response.Redirect(redirectUrl);

            // 4. Mark the response as handled to prevent the middleware from redirecting anywhere else.
            context.HandleResponse();

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactUI", policy =>
    {
        policy.WithOrigins(builder.Configuration["Frontend:BaseUrl"]!)
              .AllowAnyHeader()
              .AllowAnyMethod(); // No need for AllowCredentials with tokens
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("ReactUI");

app.UseAuthentication();
//app.UseMiddleware<UserAuthenticationMiddleware>(); // This is no longer needed with JWT
app.UseAuthorization();
app.MapControllers();

app.Run();
