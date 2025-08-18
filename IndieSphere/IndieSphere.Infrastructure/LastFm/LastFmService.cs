using IndieSphere.Domain.LastFm;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace IndieSphere.Infrastructure.LastFm;
public interface ILastFmService
{
    Task<LastFmTrackInfo> GetTrackInfo(string artist, string track);
    Task<IEnumerable<SimilarTrack>> GetSimilarSongs(string track, string artist, int limit = 20);
    Task<LastFmArtist> GetArtistInfo(string artist);
    Task<IEnumerable<SimilarArtist>> GetSimilarArtists(string artist, int limit = 10);
    Task<IEnumerable<TopTrack>> GetTopSongsByCountryAsync(string country, int limit = 20);
    Task<IEnumerable<LastFmTopTrack>> GetArtistTopTracks(string artist, int limit = 10);
    Task<IEnumerable<LastFmTopAlbum>> GetArtistTopAlbums(string artist, int limit = 10);
}
public class LastFmService : ILastFmService
{
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public LastFmService(IConfiguration config)
    {
        _configuration = config;
        _apiKey = _configuration["LastFm:ApiKey"];
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://ws.audioscrobbler.com/2.0/")
        };
    }

    public async Task<LastFmTrackInfo> GetTrackInfo(string artist, string track)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "track.getInfo",
            ["api_key"] = _apiKey,
            ["artist"] = artist,
            ["track"] = track,
            ["format"] = "json"
        };

        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LastFmTrackInfo>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<IEnumerable<SimilarTrack>> GetSimilarSongs(string track, string artist, int limit = 20)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "track.getSimilar",
            ["api_key"] = _apiKey,
            ["track"] = track,
            ["artist"] = artist,
            ["limit"] = limit.ToString(),
            ["autocorrect"] = "1",
            ["format"] = "json"
        };
        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SimilarTracksResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.SimilarTracks?.Tracks ?? Enumerable.Empty<SimilarTrack>();
    }

    public async Task<LastFmArtist> GetArtistInfo(string artist)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "artist.getinfo",
            ["api_key"] = _apiKey,
            ["artist"] = artist,
            ["format"] = "json"
        };
        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LastFmArtist>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<IEnumerable<SimilarArtist>> GetSimilarArtists(string artist, int limit = 20)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "artist.getsimilar",
            ["api_key"] = _apiKey,
            ["artist"] = artist,
            ["limit"] = limit.ToString(),
            ["autocorrect"] = "1",
            ["format"] = "json"
        };
        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SimilarArtistsResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result?.SimilarArtists?.Artists ?? Enumerable.Empty<SimilarArtist>();
    }

    public async Task<IEnumerable<TopTrack>> GetTopSongsByCountryAsync(string country, int limit = 20)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "geo.gettoptracks",
            ["api_key"] = _apiKey,
            ["country"] = country,
            ["limit"] = limit.ToString(),
            ["format"] = "json"
        };
        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TopTracksResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result?.Tracks?.Tracks ?? Enumerable.Empty<TopTrack>();
    }

    public async Task<IEnumerable<LastFmTopTrack>> GetArtistTopTracks(string artist, int limit = 10)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "artist.gettoptracks",
            ["api_key"] = _apiKey,
            ["artist"] = artist,
            ["limit"] = limit.ToString(),
            ["format"] = "json"
        };
        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ArtistTopTracksResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result?.TopTracks?.Track ?? Enumerable.Empty<LastFmTopTrack>();
    }

    public async Task<IEnumerable<LastFmTopAlbum>> GetArtistTopAlbums(string artist, int limit = 10)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "artist.gettopalbums",
            ["api_key"] = _apiKey,
            ["artist"] = artist,
            ["limit"] = limit.ToString(),
            ["format"] = "json"
        };
        var queryString = BuildQueryString(parameters);
        var response = await _httpClient.GetAsync($"?{queryString}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ArtistTopAlbumsResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return result?.TopAlbums?.Album ?? Enumerable.Empty<LastFmTopAlbum>();
    }

    private string BuildQueryString(Dictionary<string, string> parameters)
    {
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (var param in parameters)
        {
            query[param.Key] = param.Value;
        }
        return query.ToString();
    }


}
