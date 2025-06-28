using System.Text.RegularExpressions;

namespace IndieSphere.Domain.Helper;

public class SongQueryParser
{
    private static readonly Regex SongByArtistRegex = new(@"^(.*)\s+by\s+(.*)$", RegexOptions.IgnoreCase);

    public static (string Song, string Artist)? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var match = SongByArtistRegex.Match(input.Trim());
        if (match.Success && match.Groups.Count == 3)
        {
            var song = match.Groups[1].Value.Trim();
            var artist = match.Groups[2].Value.Trim();
            return (song, artist);
        }
        return null;
    }
}
