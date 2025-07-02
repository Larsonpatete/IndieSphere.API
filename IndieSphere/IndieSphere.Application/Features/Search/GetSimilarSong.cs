using IndieSphere.Domain.Helper;
using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class GetSimilarSongHandler(ILastFmService lastFm, ISpotifyService spotify) : IRequestHandler<GetSimilarSongQuery, SearchResult<Song>>
{
    private readonly ILastFmService _lastFmService = lastFm;
    private readonly ISpotifyService _spotifyService = spotify;
    public async Task<SearchResult<Song>> Handle(GetSimilarSongQuery request, CancellationToken cancellationToken)
    {
        var parsed = SongQueryParser.Parse(request.Query);
        if (parsed == null)
            throw new ArgumentException("Query must be in the format '[song] by [artist]'.");

        var similarTracks = await _lastFmService.GetSimilarSongs(parsed.Value.Song, parsed.Value.Artist, request.limit);

        // Map to Song objects
        var songs = similarTracks.Select(Song.MapLastFmTrackToSong).ToList();

        // Populate album covers using Spotify
        await _spotifyService.EnrichWithSpotify(songs);

        songs = songs.ApplyFilters(request.Filters).ToList();

        return new SearchResult<Song>
        {
            Results = songs,
            Limit = request.limit,
            TotalCount = songs.Count
        };
    }
}

public sealed record GetSimilarSongQuery(string Query, int limit, SearchFilters Filters) : IQuery<SearchResult<Song>>; // MusicQuery

