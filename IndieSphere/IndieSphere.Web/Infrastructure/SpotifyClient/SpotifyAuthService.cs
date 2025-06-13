using IndieSphere.Web.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Components;
using static System.Net.WebRequestMethods;

namespace IndieSphere.Web.Infrastructure.SpotifyClient;

public class SpotifyAuthService(IApiService api, NavigationManager nav)
{
    private readonly IApiService api = api;
    private readonly NavigationManager nav = nav;

    public async Task Login(string? returnUrl = null)
    {
        var url = $"api/spotify/login?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
        await api.GetAsync(url);
    }

    public async Task Logout()
    {
        await api.GetAsync("api/spotify/logout");
        nav.NavigateTo("/", true);
    }

    public async Task<bool> IsAuthenticated()
    {
        try
        {
            var response = await api.GetAsync<UserInfo>("spotify/me");
            return response != null && !string.IsNullOrEmpty(response.Id) && !string.IsNullOrEmpty(response.Name);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetUserName()
    {
        var response = await api.GetAsync<UserInfo>("spotify/me");
        return response?.Name;
    }

    private record UserInfo(string Id, string Name);
}
