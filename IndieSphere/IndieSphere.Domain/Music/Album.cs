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
}
