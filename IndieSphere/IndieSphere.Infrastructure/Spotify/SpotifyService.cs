using IndieSphere.Domain.Helper;
using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.ContentModeration;
using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;

namespace IndieSphere.Infrastructure.Spotify;
public interface ISpotifyService
{
    Task<SearchResult<Song>> SearchSongsAsync(string query, int limit = 20, int offset = 0);
    Task<Song> GetSongDetailsAsync(string id);
    Task EnrichWithSpotify(List<Song> songs);
    Task<SearchResult<Artist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0);
    Task<Artist> GetArtistDetailsAsync(string Id);
    Task EnrichArtistWithSpotify(List<Artist> artists);
    Task<List<Song>> GetAlbumSongs(string albumId, int limit = 30, int offset = 0);
}

public class SpotifyService(IConfiguration config, IContentModerationService contentModeration) : ISpotifyService
{
    private readonly IConfiguration _config = config;
    private readonly IContentModerationService _contentModerationService = contentModeration;
    private async Task<string> GetClientCredentialsToken()
    {
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(
            _config["Spotify:ClientId"],
            _config["Spotify:ClientSecret"]);
        var response = await new OAuthClient(config).RequestToken(request);
        return response.AccessToken;
    }
    public async Task<SearchResult<Song>> SearchSongsAsync(string query, int limit = 20, int offset = 0)
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
            return new SearchResult<Song>
            {
                TotalCount = 0,
                Offset = offset,
                Limit = limit,
                Results = new List<Song>()
            };
        }

        var songs = await Task.WhenAll(results.Tracks.Items
            .Where(t => t != null)
            .Select(async t => await MapSpotifyTrackToSong(t)));

        return new SearchResult<Song>
        {
            TotalCount = results.Tracks.Total,
            Offset = offset,
            Limit = limit,
            Results = songs.ToList()
        };
    }
    public async Task<Song> GetSongDetailsAsync(string id)
    {


        var token = await GetClientCredentialsToken();
        Console.WriteLine("Access Token: " + token);

        // Create client without 'using'
        var spotify = new SpotifyClient(token);

        // Get track info

        if (id.Contains("--"))
        {
            var parsed = SongQueryParser.ParseFallbackId(id);
            if (parsed == null)
                return null;

            var (title, artist) = parsed.Value;
            var query = $"track:\"{title}\" artist:\"{artist}\"";

            var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
            {
                Limit = 1,
                Offset = 0
            };

            var results = await spotify.Search.Item(searchRequest);
            var track = results?.Tracks?.Items?.FirstOrDefault();
            if (track == null)
                return null;

            // Use your mapping method for consistency
            return await MapSpotifyTrackToSong(track);
        }

        // Standard Spotify ID lookup
        var fullTrack = await spotify.Tracks.Get(id);
        if (fullTrack == null)
            return null;


        return await MapSpotifyTrackToSong(fullTrack);

        //try
        //{
        //    var audioFeatures = await spotify.Tracks.GetAudioFeatures(id);
        //}
        //catch (APIException ex)
        //{
        //    // Capture detailed error information
        //    var errorDetails = new StringBuilder();
        //    errorDetails.AppendLine($"Spotify API Exception: {ex.Message}");
        //    errorDetails.AppendLine($"Status Code: {ex.Response?.StatusCode}");

        //    if (ex.Response?.Headers != null)
        //    {
        //        errorDetails.AppendLine("Headers:");
        //        foreach (var header in ex.Response.Headers)
        //        {
        //            errorDetails.AppendLine($"{header.Key}: {string.Join(",", header.Value)}");
        //        }
        //    }

        //    errorDetails.AppendLine($"Response Body: {ex.Response?.Body}");

        //    // Store for debugging
        //    var errorInfo = errorDetails.ToString();
        //    Console.WriteLine(errorInfo);

        //    // Optional: Throw a more informative exception
        //    throw new Exception($"Audio features error for track {id}\n{errorInfo}", ex);
        //}
        //var album = await spotify.Albums.Get(track.Album.Id);

        //TrackAudioFeatures audioFeatures = null;
        //try
        //{
        //    audioFeatures = await spotify.Tracks.GetAudioFeatures(id);
        //}
        //catch (APIException ex)
        //{
        //    var builder = new StringBuilder();
        //    builder.AppendLine($"⚠️ Spotify API error:");
        //    builder.AppendLine($"  - Message: {ex.Message}");
        //    builder.AppendLine($"  - Status Code: {(int)ex.Response.StatusCode} {ex.Response.StatusCode}");

        //    if (ex.Response.Headers != null)
        //    {
        //        builder.AppendLine("  - Headers:");
        //        foreach (var header in ex.Response.Headers)
        //        {
        //            builder.AppendLine($"    {header.Key}: {string.Join(", ", header.Value)}");
        //        }
        //    }

        //    builder.AppendLine($"  - Body: {ex.Response.Body}");

        //    Console.WriteLine(builder.ToString());

        //    // Optional: rethrow with more detail if needed
        //    throw new Exception($"Spotify API failed getting audio features for {id}", ex);
        //}


        // Create the song with the basic mapping
        //var song = new Song
        //{
        //    Id = track.Id,
        //    Title = track.Name,
        //    Artist = new Artist
        //    {
        //        Id = track.Artists[0].Id,
        //        Name = track.Artists[0].Name,
        //        // You could get more artist details with another call if needed
        //    },
        //    Album = track.Album.Name,
        //    AlbumImageUrl = track.Album.Images.FirstOrDefault()?.Url,
        //    TrackUrl = track.ExternalUrls.ContainsKey("spotify") ? track.ExternalUrls["spotify"] : null,
        //    DurationMs = track.DurationMs,
        //    IsExplicit = track.Explicit,
        //    Popularity = track.Popularity,
        //    PreviewUrl = track.PreviewUrl,

        //    // Date handling
        //    ReleaseDate = ParseSpotifyDate(track.Album.ReleaseDate, track.Album.ReleaseDatePrecision),
        //    ReleaseDatePrecision = track.Album.ReleaseDatePrecision,

        //    // Audio features from the audio features endpoint
        //    //Energy = audioFeatures?.Energy ?? 0,
        //    //Danceability = audioFeatures?.Danceability ?? 0,
        //    //Acousticness = audioFeatures?.Acousticness ?? 0,
        //    //Instrumentalness = audioFeatures?.Instrumentalness ?? 0,
        //    //Liveness = audioFeatures?.Liveness ?? 0,
        //    //Tempo = audioFeatures?.Tempo ?? 0,
        //    //Key = audioFeatures?.Key ?? -1,

        //    // Derived data
        //    ObscurityRating = CalculateObscurityRating(track.Popularity),
        //    //MoodCategory = DetermineMood(
        //    //    audioFeatures?.Energy ?? 0,
        //    //    audioFeatures?.Valence ?? 0,
        //    //    audioFeatures?.Tempo ?? 0)
        //};

        //Add genres from the album
        //if (album != null && album.Genres?.Count > 0)
        //{
        //    song.Genres = album.Genres.Select(g => new Genre { Name = g }).ToList();
        //}

        //You could potentially make additional API calls here for Last.fm or MusicBrainz data
        // if you want to include that in this method

        //return song;
    }


    public async Task EnrichWithSpotify(List<Song> songs)
    {
        var token = await GetClientCredentialsToken();
        var spotify = new SpotifyClient(token);

        foreach (var song in songs)
        {
            // Only update if AlbumImageUrl is missing or default
            if (string.IsNullOrEmpty(song.AlbumImageUrl) || song.AlbumImageUrl.Contains("2a96cbd8b46e442fc41c2b86b821562f")) // id for no image from Last.Fm
            {
                // Use title and artist for search
                var title = song.Title;
                var artist = song.Artist?.Name;

                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(artist))
                {
                    var query = $"track:\"{title}\" artist:\"{artist}\"";
                    var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
                    {
                        Limit = 1
                    };
                    var results = await spotify.Search.Item(searchRequest);
                    var track = results?.Tracks?.Items?.FirstOrDefault();
                    var imageUrl = track?.Album?.Images?.OrderByDescending(i => i.Height).FirstOrDefault()?.Url;
                    var durationMs = track?.DurationMs;
                    var popularity = track?.Popularity;

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        //bool isExplicit = await _contentModerationService.IsImageExplicitAsync(imageUrl);
                        bool isExplicit = false;
                        if (!isExplicit)
                        {
                            song.AlbumImageUrl = imageUrl;
                            song.DurationMs = durationMs ?? song.DurationMs;
                            song.Popularity = popularity ?? song.Popularity;
                        }
                        else
                        {
                            // Log, leave blank, or use placeholder image
                            song.AlbumImageUrl = "https://example.com/placeholder-safe.jpg";
                        }
                    }
                }
            }
        }
    }

    public async Task<SearchResult<Artist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0)
    {
        var token = await GetClientCredentialsToken();
        var spotify = new SpotifyClient(token);
        var searchRequest = new SearchRequest(SearchRequest.Types.Artist, query)
        {
            Limit = limit,
            Offset = offset
        };
        var results = await spotify.Search.Item(searchRequest);
        if (results?.Artists?.Items == null)
        {
            return new SearchResult<Artist>
            {
                TotalCount = 0,
                Offset = offset,
                Limit = limit,
                Results = new List<Artist>()
            };
        }
        var artists = results.Artists.Items
            .Where(a => a != null)
            .Select(a => new Artist
            {
                Id = a.Id,
                Name = a.Name,
                Url = a.ExternalUrls?.GetValueOrDefault("spotify") ?? "",
                Genres = a.Genres.Select(g => new Genre { Name = g }).ToList(),
                Images = a.Images
                    .Where(i => i.Height >= 300) // Filter for larger images
                    .Select(i => i.Url)
                    .ToList(),
                Followers = a.Followers.Total,
                Popularity = a.Popularity

            })
            .ToList();
        return new SearchResult<Artist>
        {
            TotalCount = results.Artists.Total,
            Offset = offset,
            Limit = limit,
            Results = artists
        };
    }

    public async Task<Artist> GetArtistDetailsAsync(string id)
    {
        var token = await GetClientCredentialsToken();
        var spotify = new SpotifyClient(token);
        
        if (id.Contains("-"))
        {
            var parsedQuery = id.Replace("-", " ");
            var query = $"artist:\"{parsedQuery}\"";

            var searchRequest = new SearchRequest(SearchRequest.Types.Artist, query)
            {
                Limit = 1,
                Offset = 0
            };

            var results = await spotify.Search.Item(searchRequest);
            var artist = results?.Artists?.Items?.FirstOrDefault();
            if (artist == null)
                return null;

            // Use your mapping method for consistency
            return MapSpotifyArtistToArtist(artist);
           
        }
        var result = await spotify.Artists.Get(id);
        return MapSpotifyArtistToArtist(result);
    }


    public async Task EnrichArtistWithSpotify(List<Artist> artists)
    {
        var token = await GetClientCredentialsToken();
        var spotify = new SpotifyClient(token);

        foreach (var artist in artists)
        {
            // Only update if AlbumImageUrl is missing or default
            if (artist.Images.Count < 1 || artist.Images.Any(s => s.Contains("2a96cbd8b46e442fc41c2b86b821562f"))) // id for no image from Last.Fm
            {
                // Use title and artist for search
                var name = artist.Name;
               if (!string.IsNullOrWhiteSpace(name))
                {
                    var query = $"artist:\"{artist.Name}\"";
                    var searchRequest = new SearchRequest(SearchRequest.Types.Artist, query)
                    {
                        Limit = 1
                    };
                    var results = await spotify.Search.Item(searchRequest);
                    var newArtist = results?.Artists?.Items?.FirstOrDefault();
                    var imageUrl = newArtist?.Images[0].Url;
                    var popularity = newArtist?.Popularity;

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        artist.Images = new List<string> { imageUrl };
                        artist.Popularity = popularity ?? 0;
                    }
                }
            }
        }
    }
    public async Task<List<Song>> GetAlbumSongs(string albumId, int limit = 30, int offset = 0)
    {
        var token = await GetClientCredentialsToken();
        var spotify = new SpotifyClient(token);
        var album = await spotify.Albums.Get(albumId);
        if (album == null || album.Tracks?.Items == null)
            return new List<Song>();
        return album.Tracks.Items
            .Where(t => t != null)
            .Select(t => MapSpotifyTrackToSong(t))
            .ToList();
    }

    private async Task<Song> MapSpotifyTrackToSong(FullTrack track)
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


        // bool isExplicit = await _contentModerationService.IsImageExplicitAsync(image?.Url);

        //if (isExplicit)
        //{
        //    // Log, leave blank, or use placeholder image
        //    image = new Image { Url = "https://example.com/placeholder-safe.jpg" };
        //}

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
            Album = new Album
            {
                Id = album?.Id ?? "",
                Title = album?.Name ?? "Unknown Album",
            }, 
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

    private Artist MapSpotifyArtistToArtist(FullArtist artist)
    {
        return new Artist
        {
            Id = artist.Id,
            Name = artist.Name,
            Url = artist.ExternalUrls?.GetValueOrDefault("spotify") ?? "",
            Genres = artist.Genres.Select(g => new Genre { Name = g }).ToList(),
            Images = artist.Images
                .Where(i => i.Height >= 300) // Filter for larger images
                .Select(i => i.Url)
                .ToList(),
            Followers = artist.Followers.Total,
            Popularity = artist.Popularity
        };
    }
    private Song MapSpotifyTrackToSong(SimpleTrack track)
    {
        var primaryArtist = track.Artists?.FirstOrDefault();

        return new Song
        {
            Id = track.Id,
            Title = track.Name ?? "Unknown Track",
            Artist = new Artist
            {
                Id = primaryArtist?.Id ?? "",
                Name = primaryArtist?.Name ?? "Unknown Artist",
                Url = primaryArtist?.ExternalUrls?.GetValueOrDefault("spotify") ?? ""
            },
            // Album info is not available on SimpleTrack, so leave it empty
            // You would need to make a separate API call to get album details if needed
            Album = new Album(),
            AlbumImageUrl = null,
            TrackUrl = track.ExternalUrls?.GetValueOrDefault("spotify") ?? "",
            IsExplicit = track.Explicit,
            DurationMs = track.DurationMs,
            PreviewUrl = track.PreviewUrl,
            // Popularity and ReleaseDate are not on SimpleTrack
            Popularity = 0,
            ReleaseDate = null
        };
    }

    // Helper method to parse Spotify dates which can come in different formats
    private DateTime? ParseSpotifyDate(string date, string precision)
    {
        if (string.IsNullOrEmpty(date))
            return null;

        try
        {
            switch (precision)
            {
                case "day":
                    return DateTime.Parse(date);
                case "month":
                    return DateTime.Parse($"{date}-01");
                case "year":
                    return DateTime.Parse($"{date}-01-01");
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

    // Calculate obscurity as inverse of popularity
    private double CalculateObscurityRating(int popularity)
    {
        // Higher number = more obscure
        return 100 - popularity;
    }

    // Determine mood based on audio features
    private string DetermineMood(double energy, double valence, double tempo)
    {
        // Simple mood categorization logic
        if (valence > 0.7 && energy > 0.7) return "Happy";
        if (valence < 0.3 && energy < 0.4) return "Melancholy";
        if (energy > 0.8 && tempo > 120) return "Energetic";
        if (valence > 0.6 && energy < 0.4) return "Peaceful";
        if (valence < 0.4 && energy > 0.6) return "Angry";
        if (valence > 0.6 && tempo < 100) return "Relaxed";

        // Default mood if none of the specific categories match
        return "Balanced";
    }
    // SearchResult class definition
}
