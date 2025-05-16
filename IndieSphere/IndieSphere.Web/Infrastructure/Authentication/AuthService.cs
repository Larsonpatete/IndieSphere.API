using IndieSphere.Domain.User;
using IndieSphere.Web.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Components;

namespace IndieSphere.Web.Infrastructure.Authentication;

public class AuthService(IApiService api, NavigationManager nav)
{
    private readonly IApiService api = api;
    private readonly NavigationManager nav = nav;

    public async Task<OAuthUser> GetUser()
    {
        try
        {
            return await api.GetAsync<OAuthUser>($"api/auth/me");
        }
        catch (HttpRequestException ex)
        {
            // Handle API errors (e.g., log, show error message)
            Console.WriteLine($"Error fetching user: {ex.Message}");
            return null;
        }
    }

    public async Task Logout()
    {
        try
        {
            await api.Post("api/auth/logout");
        }
        finally
        {
            // Ensure navigation even if logout API fails
            nav.NavigateTo("/", true);
        }
    }
}
