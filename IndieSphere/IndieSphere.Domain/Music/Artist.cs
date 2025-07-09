using IndieSphere.Domain.LastFm;
using IndieSphere.Domain.Search;

namespace IndieSphere.Domain.Music;

public class Artist
{
    public string Id { get; set; }                // Unique identifier (e.g., MBID)
    public string Name { get; set; }
    public string Url { get; set; }
    public List<Genre> Genres { get; set; } = new();
    public List<string> Images { get; set; } = new(); // URLs to artist images
    public long Followers { get; set; }
    public int Popularity { get; set; }
    public string Bio { get; set; }
    public List<Song> TopTracks { get; set; } = new();
    public List<Album> TopAlbums { get; set; } = new();
    public List<Artist> SimilarArtists { get; set; } = new();
    public string Match { get; set; }

    public static Artist MapLastFmArtist(SimilarArtist artist)
    {
        return new Artist
        {
            Id = (artist.Name?.Replace(' ', '-') + "-"),
            Name = artist.Name,
            Url = artist.Url,
            Images = artist.Images?.Select(img => img.Url).Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>(),
            Genres = new List<Genre>(),
            Followers = 0,
            Popularity = 0,
            Match = artist.Match
        };
    }
}

public static class ArtistExtensions
{
    public static IEnumerable<Artist> ApplyFilters(this IEnumerable<Artist> artists, SearchFilters filters)
    {
        if (filters == null) return artists;

        if (filters.MinPopularity.HasValue)
            artists = artists.Where(song => song.Popularity >= filters.MinPopularity.Value);

        if (filters.MaxPopularity.HasValue)
            artists = artists.Where(song => song.Popularity <= filters.MaxPopularity.Value);

        if (!string.IsNullOrEmpty(filters.Genre))
            artists = artists.Where(song => song.Genres.Any(genre => genre.Name.Equals(filters.Genre, StringComparison.OrdinalIgnoreCase)));

        return artists;
    }
}
