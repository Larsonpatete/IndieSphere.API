namespace IndieSphere.Domain.Music;

public class MusicQuery
{
    public SearchIntent Intent { get; set; } = SearchIntent.Unknown;
    public List<QueryEntity> Entities { get; set; } = new List<QueryEntity>();

    public string PrimaryEntity => Entities.FirstOrDefault()?.Text ?? string.Empty;
}

public record QueryEntity(EntityType Type, string Text);

public enum SearchIntent
{
    SimilarSongSearch,
    ArtistSearch,
    GenreSearch,
    AlbumSearch,
    SongSearch,
    SimilarArtistSearch,
    Unknown
}

public enum EntityType
{
    Artist,
    Song,
    Album,
    Genre,
    Unknown
}

