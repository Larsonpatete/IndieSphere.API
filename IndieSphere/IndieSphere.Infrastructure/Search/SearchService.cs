using IndieSphere.Domain.Music;
using System.Text.Json;

namespace IndieSphere.Infrastructure.Search;

public interface ISearchService
{
    Task<SearchResult<Song>> SearchSongsAsync(string query, int limit);
    Task<SearchResult<Artist>> SearchArtistsAsync(string name);
    Task<SearchResult<Song>> SearchByGenreAsync(string genre);
    Task<Song> GetSongAsync(string title, string artist);
    Task<Artist> GetArtistAsync(string name);
}
public class SearchService : ISearchService
{
    private readonly HttpClient _httpClient;
    public SearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Set a custom User-Agent as required by MusicBrainz
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("IndieSphere/1.0 (larsonpatete@gmail.com)");
    }
    public async Task<SearchResult<Song>> SearchSongsAsync(string query, int limit)
    {
        var url = $"https://musicbrainz.org/ws/2/recording/?query={Uri.EscapeDataString(query)}&limit={limit}&fmt=json";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var songs = new List<Song>();
        int totalCount = 0;

        if (doc.RootElement.TryGetProperty("count", out var countProp))
        {
            totalCount = countProp.GetInt32();
        }

        if (doc.RootElement.TryGetProperty("recordings", out var recordings))
        {
            foreach (var rec in recordings.EnumerateArray())
            {
                var title = rec.GetProperty("title").GetString();
                var id = rec.GetProperty("id").GetString();
                var artistCredit = rec.GetProperty("artist-credit")[0];
                var artistName = artistCredit.GetProperty("name").GetString();
                var artistId = artistCredit.GetProperty("artist").GetProperty("id").GetString();

                songs.Add(new Song
                {
                    Id = id,
                    Title = title,
                    Artist = new Artist
                    {
                        Id = artistId,
                        Name = artistName,
                        Url = $"https://musicbrainz.org/artist/{artistId}"
                    }
                });
            }
        }

        return new SearchResult<Song>(songs, totalCount)
        {
            TotalCount = totalCount,
            Results = songs
        };
    }

    public Task<SearchResult<Artist>> SearchArtistsAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<SearchResult<Song>> SearchByGenreAsync(string genre)
    {
        throw new NotImplementedException();
    }

    public async Task<Artist> GetArtistAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<Song> GetSongAsync(string title, string artist)
    {
        throw new NotImplementedException();
    }
}

public sealed record SearchResult<T>(List<T> Results, int TotalCount)
{
    public List<T> Results { get; init; } = Results;
    public int TotalCount { get; init; } = TotalCount;
}
