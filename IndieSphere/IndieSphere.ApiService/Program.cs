using Azure.Identity;
using IndieSphere.Application;
using IndieSphere.Infrastructure;
using IndieSphere.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("IndieSphereDb");

builder.Services
    .AddProblemDetails()
    .AddOpenApi()
    .AddHttpContextAccessor()
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(connectionString)
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
    options.SaveTokens = true;

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // 1. Get the access token and fetch the user profile from Spotify.
            var accessToken = context.AccessToken;
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            // 2. Create the claims for your application's identity.
            using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var imageUrl = user.RootElement.TryGetProperty("images", out var images) && images.GetArrayLength() > 0
                ? images[0].GetProperty("url").GetString()
                : null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.RootElement.GetString("id")),
                new Claim(ClaimTypes.Name, user.RootElement.GetString("display_name")),
                new Claim(ClaimTypes.Email, user.RootElement.GetString("email")),
                new Claim("urn:spotify:profile_image_url", imageUrl ?? "")
                //new Claim("urn:spotify:access_token", accessToken)
            };

            // 3. Replace the context principal with your new claims identity.
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
            var redirectUrl = $"http://localhost:3000/login-success?token={jwtToken}";

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
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod(); // No need for AllowCredentials with tokens
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

app.UseCors("ReactUI");

app.UseAuthentication();
//app.UseMiddleware<UserAuthenticationMiddleware>(); // This is no longer needed with JWT
app.UseAuthorization();
app.MapControllers();

app.Run();
