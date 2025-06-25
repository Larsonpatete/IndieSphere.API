using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using static IndieSphere.Infrastructure.LastFm.LastFmService;

namespace IndieSphere.Infrastructure.LastFm;
public interface ILastFmService
{
    Task<LastFmTrackInfo> GetTrackInfo(string artist, string track);
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
    public class LastFmTrackInfo
    {
        [JsonPropertyName("track")]
        public LastFmTrack Track { get; set; }
    }

    public class LastFmTrack
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("artist")]
        public LastFmArtist Artist { get; set; }

        [JsonPropertyName("album")]
        public LastFmAlbum Album { get; set; }

        [JsonPropertyName("playcount")]
        public string PlaycountString { get; set; }

        [JsonPropertyName("listeners")]
        public string ListenersString { get; set; }

        [JsonIgnore]
        public long Playcount => long.TryParse(PlaycountString, out var result) ? result : 0;

        [JsonIgnore]
        public long Listeners => long.TryParse(ListenersString, out var result) ? result : 0;

        [JsonPropertyName("wiki")]
        public LastFmWiki Wiki { get; set; }

        [JsonPropertyName("toptags")]
        public LastFmTopTags TopTags { get; set; }
    }

    public class LastFmArtist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mbid")]
        public string MusicBrainzId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class LastFmAlbum
    {
        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("image")]
        public List<LastFmImage> Images { get; set; }
    }

    public class LastFmImage
    {
        [JsonPropertyName("#text")]
        public string Url { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }
    }

    public class LastFmWiki
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class LastFmTopTags
    {
        [JsonPropertyName("tag")]
        public List<LastFmTag> Tags { get; set; }
    }

    public class LastFmTag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
