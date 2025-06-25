using IndieSphere.Domain.Music;
using System.Text.Json;
using static IndieSphere.Infrastructure.Spotify.SpotifyService;

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
    //public async Task<SearchResult<Song>> SearchSongsAsync(string query, int limit)
    //{
    //    var url = $"https://musicbrainz.org/ws/2/recording/?query={Uri.EscapeDataString(query)}&limit={limit}&fmt=json";
    //    var response = await _httpClient.GetAsync(url);
    //    response.EnsureSuccessStatusCode();

    //    var json = await response.Content.ReadAsStringAsync();
    //    var doc = JsonDocument.Parse(json);

    //    var songs = new List<Song>();
    //    int totalCount = doc.RootElement.GetProperty("count").GetInt32();

    //    if (doc.RootElement.TryGetProperty("recordings", out var recordings))
    //    {
    //        // First pass: collect all release IDs
    //        var releaseIds = new List<string>();
    //        var songData = new List<Song>();

    //        foreach (var rec in recordings.EnumerateArray())
    //        {
    //            var (song, releaseId) = ParseSongData(rec);
    //            songData.Add((song, releaseId));

    //            if (!string.IsNullOrEmpty(releaseId))
    //            {
    //                releaseIds.Add(releaseId);
    //            }
    //        }

    //        // Batch check cover art in parallel
    //        var coverArtLookup = await CheckCoverArtBatch(releaseIds.Distinct());

    //        // Second pass: assign cover art URLs
    //        foreach (var (song, releaseId) in songData)
    //        {
    //            if (!string.IsNullOrEmpty(releaseId)
    //                && coverArtLookup.TryGetValue(releaseId, out var coverUrl))
    //            {
    //                song.AlbumImageUrl = coverUrl;
    //            }

    //            songs.Add(song);
    //        }
    //    }

    //    return new SearchResult<Song>(songs, totalCount);
    //}

    //private (Song song, string releaseId) ParseSongData(JsonElement rec)
    //{
    //    var title = rec.GetProperty("title").GetString();
    //    var id = rec.GetProperty("id").GetString();

    //    // Artist mapping
    //    Artist? songArtist = null;
    //    string? releaseId = null;

    //    if (rec.TryGetProperty("artist-credit", out var artistCredits) &&
    //        artistCredits.GetArrayLength() > 0)
    //    {
    //        var firstArtist = artistCredits[0];
    //        var artistProp = firstArtist.GetProperty("artist");
    //        string artistId = artistProp.GetProperty("id").GetString()!;

    //        songArtist = new Artist
    //        {
    //            Id = artistId,
    //            Name = firstArtist.GetProperty("name").GetString() ?? "Unknown Artist",
    //            Url = $"https://musicbrainz.org/artist/{artistId}"
    //        };
    //    }

    //    // Album mapping
    //    string? album = null;
    //    if (rec.TryGetProperty("releases", out var releases) && releases.GetArrayLength() > 0)
    //    {
    //        var firstRelease = releases[0];
    //        album = firstRelease.GetProperty("release-group")
    //            .GetProperty("title").GetString();

    //        releaseId = firstRelease.GetProperty("id").GetString()!;
    //    }

    //    return (new Song
    //    {
    //        Id = id!,
    //        Title = title!,
    //        Artist = songArtist ?? new Artist { Name = "Unknown Artist" },
    //        Album = album,
    //        TrackUrl = $"https://musicbrainz.org/recording/{id}",
    //        Genres = new List<Genre>(),
    //        IsExplicit = false
    //    }, releaseId);
    //}

    //private async Task<Dictionary<string, string>> CheckCoverArtBatch(IEnumerable<string> releaseIds)
    //{
    //    var results = new Dictionary<string, string>();
    //    var tasks = new List<Task>();
    //    var throttle = new SemaphoreSlim(initialCount: 10); // Limit concurrent requests

    //    foreach (var releaseId in releaseIds)
    //    {
    //        await throttle.WaitAsync(); // Rate limiting

    //        tasks.Add(Task.Run(async () =>
    //        {
    //            try
    //            {
    //                var url = $"https://coverartarchive.org/release/{releaseId}/front-250.jpg";
    //                using var request = new HttpRequestMessage(HttpMethod.Head, url);

    //                // Use a short timeout
    //                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    //                var response = await _httpClient.SendAsync(request, cts.Token);

    //                if (response.IsSuccessStatusCode)
    //                {
    //                    lock (results)
    //                    {
    //                        results[releaseId] = url;
    //                    }
    //                }
    //            }
    //            catch { /* Ignore errors */ }
    //            finally
    //            {
    //                throttle.Release();
    //            }
    //        }));
    //    }

    //    await Task.WhenAll(tasks);
    //    return results;
    //}
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
                // Artist mapping with null checks
                Artist? songArtist = null;
                if (rec.TryGetProperty("artist-credit", out var artistCredits) && artistCredits.GetArrayLength() > 0)
                {
                    var firstArtist = artistCredits[0];
                    string artistId = firstArtist.GetProperty("artist")
                        .GetProperty("id").GetString()!;

                    songArtist = new Artist
                    {
                        Id = artistId,
                        Name = firstArtist.GetProperty("name").GetString() ?? "Unknown Artist",
                        Url = $"https://musicbrainz.org/artist/{artistId}"
                    };
                }


                // Album mapping
                string? album = null;
                string? albumImageUrl = null;
                if (rec.TryGetProperty("releases", out var releases) && releases.GetArrayLength() > 0)
                {
                    var firstRelease = releases[0];
                    album = firstRelease.GetProperty("release-group")
                        .GetProperty("title").GetString();

                    string releaseId = firstRelease.GetProperty("id").GetString()!;
                    albumImageUrl = await GetAlbumCoverUrl(releaseId);
                }

                var song = new Song
                {
                    Id = id,
                    Title = title,
                    Artist = songArtist ?? new Artist { Name = "Unknown Artist" },
                    Album = album,
                    AlbumImageUrl = albumImageUrl,
                    TrackUrl = $"https://musicbrainz.org/recording/{id}",
                    Genres = new List<Genre>(), // MusicBrainz doesn't provide genres at recording level
                    IsExplicit = false // Not provided by MusicBrainz API
                };

                songs.Add(song);
            }
        }

        return new SearchResult<Song>(songs, totalCount)
        {
            TotalCount = totalCount,
            Results = songs
        };
    }

    public async Task<string> GetAlbumCoverUrl(string releaseId) // handles not found URLs
    {
        var url = $"https://coverartarchive.org/release/{releaseId}/front-250.jpg";

        using var client = new HttpClient();
        try
        {
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            return response.IsSuccessStatusCode ? url : null;
        }
        catch
        {
            return null;
        }
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

public sealed record SongResult(Song song);
