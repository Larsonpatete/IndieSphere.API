using IndieSphere.Domain.LastFm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndieSphere.Domain.Music;

public class Album
{
    public string Id { get; set; }                // Unique identifier (e.g., MBID)
    public string Title { get; set; }
    public Artist Artist { get; set; }
    public string ReleaseDate { get; set; } // ISO 8601 format (e.g., "2023-10-01")
    public string ReleaseDatePrecision { get; set; } // "day", "month", "year"
    public string CoverImageUrl { get; set; } // URL to cover art image
    public string AlbumUrl { get; set; } // URL to album page (e.g., on MusicBrainz or Spotify)
    public List<Song> Songs { get; set; } = new(); // List of songs in the album

    public static Album MapLastFmTopAlbumToAlbum(LastFmTopAlbum album)
    {
        if (album == null) return null;

        // Get the largest image (if available)
        string coverImageUrl = album.Image?
            .OrderByDescending(img => img.Size) // You may want to adjust this if Size is not ordinal
            .FirstOrDefault()?.Url;

        return new Album
        {
            Id = album.Artist?.MusicBrainzId, // Use MBID if available from artist
            Title = album.Name,
            Artist = album.Artist != null
                ? new Artist
                {
                    Name = album.Artist.Name,
                    Id = album.Artist.MusicBrainzId,
                    Url = album.Artist.Url
                }
                : null,
            ReleaseDate = null, // Not available in LastFmTopAlbum
            ReleaseDatePrecision = null, // Not available in LastFmTopAlbum
            CoverImageUrl = coverImageUrl,
            AlbumUrl = album.Url,
            Songs = new List<Song>() // No track list in LastFmTopAlbum
        };
    }
}
