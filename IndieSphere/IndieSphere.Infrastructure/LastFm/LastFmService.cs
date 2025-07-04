using IndieSphere.Domain.LastFm;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using static IndieSphere.Infrastructure.LastFm.LastFmService;

namespace IndieSphere.Infrastructure.LastFm;
public interface ILastFmService
{
    Task<LastFmTrackInfo> GetTrackInfo(string artist, string track);
    Task<IEnumerable<SimilarTrack>> GetSimilarSongs(string track, string artist, int limit = 20);
    Task<LastFmArtist> GetArtistInfo(string artist);
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
            BaseAddress = new Uri("http://ws.audioscrobbler.com/2.0/")
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

    private string BuildQueryString(Dictionary<string, string> parameters)
    {
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (var param in parameters)
        {
            query[param.Key] = param.Value;
        }
        return query.ToString();
    }

    // Response DTOs with JsonPropertyName attributes
  
}
