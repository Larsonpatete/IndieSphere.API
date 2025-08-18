using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IndieSphere.Infrastructure.Security;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? GetSpotifyAccessToken();
}

public class HttpContextUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    //public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier);
    public string? UserId => "";

    public string? GetSpotifyAccessToken()
    {
        return UserId; // omplement later
        // The token is now a claim within our own JWT.
        //return User?.FindFirst("urn:spotify:access_token");
    }
}