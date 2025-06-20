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
    options.ClientId = builder.Configuration["Spotify:ClientId"];
    options.ClientSecret = builder.Configuration["Spotify:ClientSecret"];
    options.CallbackPath = new PathString("/api/callback");

    // Spotify OAuth endpoints
    options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    options.TokenEndpoint = "https://accounts.spotify.com/api/token";
    options.UserInformationEndpoint = "https://api.spotify.com/v1/me";

    // Add scopes
    options.Scope.Add("user-top-read");
    options.Scope.Add("user-read-recently-played");

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
