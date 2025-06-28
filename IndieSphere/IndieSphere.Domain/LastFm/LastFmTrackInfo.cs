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
