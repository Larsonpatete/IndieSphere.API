using IndieSphere.Domain.Music;
using IndieSphere.Web.Infrastructure.ApiClient;

namespace IndieSphere.Web.Infrastructure.SpotifyClient;

public class SpotifyClient(IApiService api)
{
    private readonly IApiService api = api;

    // Get Spotify recommendations for a given artist query
    public async Task<IEnumerable<Song?>> Search(string artistQuery)
    {
        return await api.GetAsync<IEnumerable<Song?>>($"api/spotify/search?query={artistQuery}");
    }
    // Other Spotify-related methods can be added here
}