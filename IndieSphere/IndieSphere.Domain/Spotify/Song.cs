using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndieSphere.Domain.Spotify;

public class Song
{
    public string name { get; set; }
    public string artist { get; set; }
    public Uri artistLink { get; set; }
    public Uri songLink { get; set; }
    public Uri coverArtLink { get; set; }

    public Song(string name, string artist, Uri artistLink, Uri songLink, Uri coverArtLink)
    {
        this.name = name;
        this.artist = artist;
        this.artistLink = artistLink;
        this.songLink = songLink;
        this.coverArtLink = coverArtLink;
    }
}
