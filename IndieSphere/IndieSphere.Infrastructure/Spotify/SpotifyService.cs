using IndieSphere.Domain.Music;
using IndieSphere.Infrastructure.Search;
using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;
using System.Collections.Generic;
using static IndieSphere.Infrastructure.Spotify.SpotifyService;

namespace IndieSphere.Infrastructure.Spotify;
public interface ISpotifyService
{
    Task<SpotifySearchResult<Song>> SearchSongsAsync(string query, int limit = 20, int offset = 0);
}

public class SpotifyService(IConfiguration config) : ISpotifyService
{
    private readonly IConfiguration _config = config;
    private async Task<string> GetClientCredentialsToken()
    {
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(
            _config["Spotify:ClientId"],
            _config["Spotify:ClientSecret"]);
        var response = await new OAuthClient(config).RequestToken(request);
        return response.AccessToken;
    }
    public async Task<SpotifySearchResult<Song>> SearchSongsAsync(string query, int limit = 20, int offset = 0)
    {
        var token = await GetClientCredentialsToken();
        var spotify = new SpotifyClient(token);

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
        {
            Limit = limit,
            Offset = offset
        };

        var results = await spotify.Search.Item(searchRequest);

        if (results?.Tracks?.Items == null)
        {
            return new SpotifySearchResult<Song>
            {
                TotalCount = 0,
                Offset = offset,
                Limit = limit,
                Results = new List<Song>()
            };
        }

        var songs = results.Tracks.Items
            .Where(t => t != null)
            .Select(t => MapSpotifyTrackToSong(t))
            .ToList();

        return new SpotifySearchResult<Song>
        {
            TotalCount = results.Tracks.Total,
            Offset = offset,
            Limit = limit,
            Results = songs
        };
    }
    private Song MapSpotifyTrackToSong(FullTrack track)
    {
        var album = track.Album;
        var primaryArtist = track.Artists?.FirstOrDefault();

        // Parse release date
        DateTime? releaseDate = null;
        if (!string.IsNullOrEmpty(album?.ReleaseDate) &&
            DateTime.TryParse(album.ReleaseDate, out var parsedDate))
        {
            releaseDate = parsedDate;
        }

        // Get best available image
        var image = album?.Images
            ?.OrderByDescending(i => i.Height)
            .FirstOrDefault(i => i.Height >= 300)
            ?? album?.Images?.FirstOrDefault();

        return new Song
        {
            Id = track.Id,
            Title = track.Name ?? "Unknown Track",
            Artist = new Artist
            {
                Id = primaryArtist?.Id ?? "",
                Name = primaryArtist?.Name ?? "Unknown Artist",
                Url = primaryArtist?.ExternalUrls?.GetValueOrDefault("spotify") ?? "",
                Genres = new List<Genre>()
            },
            Album = album?.Name ?? "",
            AlbumImageUrl = image?.Url,
            TrackUrl = track.ExternalUrls?.GetValueOrDefault("spotify") ?? "",
            Genres = new List<Genre>(),
            IsExplicit = track.Explicit,
            DurationMs = track.DurationMs,
            ReleaseDate = releaseDate,
            ReleaseDatePrecision = album?.ReleaseDatePrecision,
            Popularity = track.Popularity,
            PreviewUrl = track.PreviewUrl
        };
    }

    // SearchResult class definition
    public class SpotifySearchResult<T>
    {
        public int? TotalCount { get; set; }       // Total items available
        public int Offset { get; set; }           // Current starting index
        public int Limit { get; set; }            // Max items per page
        public List<T> Results { get; set; }      // Current page results
    }
}
