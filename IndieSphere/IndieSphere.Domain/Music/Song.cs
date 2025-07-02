using IndieSphere.Domain.LastFm;
using IndieSphere.Domain.Search;

namespace IndieSphere.Domain.Music;

public class Song
{
    public string Id { get; set; }                // Unique identifier (e.g., MBID)
    public string Title { get; set; }
    public Artist Artist { get; set; }
    public string Album { get; set; }
    public string AlbumImageUrl { get; set; }
    public string TrackUrl { get; set; }
    public List<Genre> Genres { get; set; } = new();
    public bool IsExplicit { get; set; }
    public int DurationMs { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string ReleaseDatePrecision { get; set; }
    public int Popularity { get; set; }          // Spotify's 0-100 popularity score
    public string PreviewUrl { get; set; }

    // Additional properties for indie music discovery

    // Last.fm data
    public long? PlayCount { get; set; }           // Number of plays on Last.fm
    public long? ListenerCount { get; set; }       // Unique listeners on Last.fm
    public List<Tag> UserTags { get; set; } = new();  // User-generated tags (beyond formal genres)
    public string Description { get; set; }      // Wiki-style description from Last.fm
    public double? SimilarSongMatch { get; set; } // 0.0-1.0 score for match similarity

    // Underground appeal metrics
    public double ObscurityRating { get; set; }  // Inverse of popularity, helpful for finding hidden gems
    public bool IsIndieLabelRelease { get; set; } // Released on an independent label vs major

    // Audio characteristics (from Spotify)
    public double Energy { get; set; }           // 0.0 to 1.0 - how energetic the track is
    public double Danceability { get; set; }     // 0.0 to 1.0 - how suitable for dancing
    public double Acousticness { get; set; }     // 0.0 to 1.0 - confidence the track is acoustic
    public double Instrumentalness { get; set; } // 0.0 to 1.0 - predicts if track contains no vocals
    public double Liveness { get; set; }         // 0.0 to 1.0 - presence of audience in recording
    public float Tempo { get; set; }               // Estimated tempo in BPM
    public int Key { get; set; }                 // The key the track is in

    // Music critic and community data
    public List<Review> Reviews { get; set; } = new(); // Critics or user reviews (Pitchfork, etc.)
    public double CommunityRating { get; set; } // Average rating from users (0-5 stars)

    // Relationship data (from MusicBrainz)
    public bool IsCover { get; set; }            // Is this a cover of another song
    public string OriginalArtist { get; set; }   // If it's a cover, who did the original
    public string RecordLabel { get; set; }      // Label that released the song/album

    // Discovery context
    public List<string> FeaturedInPlaylists { get; set; } = new(); // Notable playlists featuring this song
    public List<Song> SimilarSongs { get; set; } = new(); // Songs that listeners also enjoyed
    public bool IsBreakoutTrack { get; set; }    // The artist's most successful track
    public string MoodCategory { get; set; }     // Categorization like "chill", "energetic", "melancholy"

    // Indie-specific attributes
    public bool HasVinylRelease { get; set; }    // Available on vinyl (popular in indie communities)
    public bool IsDIY { get; set; }              // Self-produced/published
    public bool HasLiveVersions { get; set; }    // Live recordings available
    public int UndergroundBuzzScore { get; set; } // Measure of discussion in indie communities


    public static Song MapLastFmTrackToSong(SimilarTrack track)
    {
        // Use lowercased, trimmed, and replace spaces with dashes for uniqueness
        var title = track.Name?.Trim().ToLowerInvariant().Replace(" ", "-") ?? "";
        var artist = track.Artist?.Name?.Trim().ToLowerInvariant().Replace(" ", "-") ?? "";
        var fallbackId = $"{title}--{artist}";

        return new Song
        {
            Title = track.Name,
            TrackUrl = track.Url,
            Artist = track.Artist != null ? new Artist
            {
                Name = track.Artist.Name,
                Url = track.Artist.Url,
                Id = track.Artist.MusicBrainzId
            } : null,
            AlbumImageUrl = track.Images?.FirstOrDefault(i => i.Size == "large")?.Url
                         ?? track.Images?.FirstOrDefault()?.Url,
            Id = fallbackId,
            SimilarSongMatch = track.Match
        };
    }

    public void EnrichWithLastFm(LastFmTrack lastFmTrack)
    {
        if (lastFmTrack == null) return;

        PlayCount = lastFmTrack.Playcount;
        ListenerCount = lastFmTrack.Listeners;
        Description = lastFmTrack.Wiki?.Content;

        // Add user tags as UserTags
        if (lastFmTrack.TopTags?.Tags != null && lastFmTrack.TopTags.Tags.Any())
        {
            UserTags = lastFmTrack.TopTags.Tags
                .Select(t => new Tag { Name = t.Name })
                .ToList();

            // Optionally, add tags as genres if not already present
            if (Genres == null)
                Genres = new List<Genre>();

            foreach (var tag in lastFmTrack.TopTags.Tags)
            {
                if (!Genres.Any(g => g.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
                    Genres.Add(new Genre { Name = tag.Name });
            }
        }
    }
}

// New supporting classes
public class Tag
{
    public string Name { get; set; }
    public int Count { get; set; }  // How many users applied this tag
}

public class Review
{
    public string Source { get; set; }  // Pitchfork, user, etc.
    public string Content { get; set; }
    public double? Rating { get; set; }
    public DateTime Date { get; set; }
}

public static class SongExtensions
{
    public static IEnumerable<Song> ApplyFilters(this IEnumerable<Song> songs, SearchFilters filters)
    {
        if (filters == null) return songs;

        if (filters.MinPopularity.HasValue)
            songs = songs.Where(song => song.Popularity >= filters.MinPopularity.Value);

        if (filters.MaxPopularity.HasValue)
            songs = songs.Where(song => song.Popularity <= filters.MaxPopularity.Value);

        if (!string.IsNullOrEmpty(filters.Genre))
            songs = songs.Where(song => song.Genres.Any(genre => genre.Name.Equals(filters.Genre, StringComparison.OrdinalIgnoreCase)));

        if (filters.MinYear.HasValue)
            songs = songs.Where(song => song.ReleaseDate.HasValue && song.ReleaseDate.Value.Year >= filters.MinYear.Value);

        if (filters.MaxYear.HasValue)
            songs = songs.Where(song => song.ReleaseDate.HasValue && song.ReleaseDate.Value.Year <= filters.MaxYear.Value);

        if (filters.MinTempo.HasValue)
            songs = songs.Where(song => song.Tempo >= filters.MinTempo.Value);

        if (filters.MaxTempo.HasValue)
            songs = songs.Where(song => song.Tempo <= filters.MaxTempo.Value);

        return songs;
    }
}