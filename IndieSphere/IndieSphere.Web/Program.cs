using IndieSphere.Web;
using IndieSphere.Web.Infrastructure.ApiClient;
using IndieSphere.Web.Infrastructure.Authentication;
using IndieSphere.Web.Infrastructure.SpotifyClient;
using IndieSphere.Web.Infrastructure.UserClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

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
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddCascadingAuthenticationState();



builder.Services
    .AddTransient<UserClient>()
    .AddTransient<AuthService>()
    .AddTransient<SpotifyClient>()
    .AddTransient<SpotifyAuthService>()
    ;

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
