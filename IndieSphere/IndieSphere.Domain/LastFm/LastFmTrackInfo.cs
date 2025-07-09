using System.Text.Json.Serialization;

namespace IndieSphere.Domain.LastFm;

public class LastFmTrackInfo
{
    [JsonPropertyName("track")]
    public LastFmTrack Track { get; set; }
}

public class LastFmTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("artist")]
    public LastFmArtist Artist { get; set; }

    [JsonPropertyName("album")]
    public LastFmAlbum Album { get; set; }

    [JsonPropertyName("playcount")]
    public string PlaycountString { get; set; }

    [JsonPropertyName("listeners")]
    public string ListenersString { get; set; }

    [JsonIgnore]
    public long Playcount => long.TryParse(PlaycountString, out var result) ? result : 0;

    [JsonIgnore]
    public long Listeners => long.TryParse(ListenersString, out var result) ? result : 0;

    [JsonPropertyName("wiki")]
    public LastFmWiki Wiki { get; set; }

    [JsonPropertyName("toptags")]
    public LastFmTopTags TopTags { get; set; }

    [JsonPropertyName("duration")]
    public string DurationString { get; set; }
}

public class LastFmArtist
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mbid")]
    public string MusicBrainzId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class LastFmAlbum
{
    [JsonPropertyName("artist")]
    public string Artist { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("image")]
    public List<LastFmImage> Images { get; set; }
}

public class LastFmImage
{
    [JsonPropertyName("#text")]
    public string Url { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }
}

public class LastFmWiki
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class LastFmTopTags
{
    [JsonPropertyName("tag")]
    public List<LastFmTag> Tags { get; set; }
}

public class LastFmTag
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class SimilarTracksResponse
{
    [JsonPropertyName("similartracks")]
    public SimilarTracks SimilarTracks { get; set; }
}

public class SimilarTracks
{
    [JsonPropertyName("track")]
    public List<SimilarTrack> Tracks { get; set; }
}

public class SimilarTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mbid")]
    public string MusicBrainzId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("match")]
    public double? Match { get; set; }

    [JsonPropertyName("artist")]
    public LastFmArtist Artist { get; set; }

    [JsonPropertyName("image")]
    public List<LastFmImage> Images { get; set; }
}

// DTOs for deserialization
public class SimilarArtistsResponse
{
    [JsonPropertyName("similarartists")]
    public SimilarArtistsContainer SimilarArtists { get; set; }
}

public class SimilarArtistsContainer
{
    [JsonPropertyName("artist")]
    public List<SimilarArtist> Artists { get; set; }
}

public class SimilarArtist
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mbid")]
    public string MusicBrainzId { get; set; }

    [JsonPropertyName("match")]
    public string Match { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("image")]
    public List<LastFmArtistImage> Images { get; set; }
}

public class LastFmArtistImage
{
    [JsonPropertyName("#text")]
    public string Url { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }
}

public class TopTracksResponse
{
    [JsonPropertyName("toptracks")]
    public TopTracksContainer TopTracks { get; set; }

    // This property can also be 'tracks' for other endpoints like chart.getTopTracks
    [JsonPropertyName("tracks")]
    public TopTracksContainer Tracks { get; set; }
}

public class TopTracksContainer
{
    [JsonPropertyName("track")]
    public List<TopTrack> Tracks { get; set; }

    [JsonPropertyName("@attr")]
    public TopTracksAttributes Attributes { get; set; }
}

public class TopTracksAttributes
{
    [JsonPropertyName("country")]
    public string Country { get; set; }
}

public class TopTrack
{
    [JsonPropertyName("@attr")]
    public TrackAttributes Attributes { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("playcount")]
    public string Playcount { get; set; }
    
    [JsonPropertyName("listeners")]
    public string Listeners { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("mbid")]
    public string MusicBrainzId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("streamable")]
    public LastFmStreamable Streamable { get; set; }

    [JsonPropertyName("artist")]
    public LastFmArtist Artist { get; set; }

    [JsonPropertyName("image")]
    public List<LastFmImage> Images { get; set; }
}

public class TrackAttributes
{
    [JsonPropertyName("rank")]
    public string Rank { get; set; }
}

public class LastFmStreamable
{
    [JsonPropertyName("#text")]
    public string IsStreamable { get; set; }

    [JsonPropertyName("fulltrack")]
    public string IsFullTrack { get; set; }
}




// Response DTOs with JsonPropertyName attributes
public class ArtistTopTracksResponse
{
    [JsonPropertyName("toptracks")]
    public TopTracksList TopTracks { get; set; }
}

public class TopTracksList
{
    [JsonPropertyName("track")]
    public List<LastFmTopTrack> Track { get; set; }
}

public class LastFmTopTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("playcount")]
    public string Playcount { get; set; }
    [JsonPropertyName("listeners")]
    public string Listeners { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("artist")]
    public LastFmArtist Artist { get; set; }
    [JsonPropertyName("image")]
    public List<LastFmImage> Image { get; set; }
}

public class ArtistTopAlbumsResponse
{
    [JsonPropertyName("topalbums")]
    public TopAlbumsList TopAlbums { get; set; }
}

public class TopAlbumsList
{
    [JsonPropertyName("album")]
    public List<LastFmTopAlbum> Album { get; set; }
}

public class LastFmTopAlbum
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("playcount")]
    public int Playcount { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("artist")]
    public LastFmArtist Artist { get; set; }
    [JsonPropertyName("image")]
    public List<LastFmImage> Image { get; set; }
}