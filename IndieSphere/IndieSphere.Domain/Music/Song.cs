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
}
