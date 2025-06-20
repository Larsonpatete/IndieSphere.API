
namespace IndieSphere.Domain.Music;

public class Artist
{
    public string Id { get; set; }                // Unique identifier (e.g., MBID)
    public string Name { get; set; }
    public string Url { get; set; }
    public List<Genre> Genres { get; set; } = new();
}
