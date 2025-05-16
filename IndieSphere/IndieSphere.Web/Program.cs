using IndieSphere.Web;
using IndieSphere.Web.Infrastructure.ApiClient;
using IndieSphere.Web.Infrastructure.Authentication;
using IndieSphere.Web.Infrastructure.UserClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(); 

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7598"); // Your API URL
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add authentication state management
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();



builder.Services
    .AddTransient<UserClient>()
    .AddTransient<AuthService>()
    ;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.SaveTokens = true; // Important to save tokens

        //// Explicitly set the callback path
        //options.CallbackPath = new PathString("/api/auth/callback");

        //// Required for proper claim mapping
        //options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        //options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        //options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    });

builder.Services.AddAuthorization();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.Run();
