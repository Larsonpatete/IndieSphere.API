using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class GetTopSongsByCountryHandler(ISpotifyService spotifyService, ILastFmService last) : IRequestHandler<GetTopSongsByCountryQuery, SearchResult<Song>>
{
    private readonly ISpotifyService _spotifyService = spotifyService;
    private readonly ILastFmService _lastFmService = last;
    public async Task<SearchResult<Song>> Handle(GetTopSongsByCountryQuery request, CancellationToken cancellationToken)
    {
        var songs = await _lastFmService.GetTopSongsByCountryAsync(request.Country, request.limit);
        return new SearchResult<Song>
        {
            Results = songs.Select(Song.MapLastFmTopTrackToSong).ToList(),
            TotalCount = songs.Count(),
            Limit = request.limit,
            Offset = request.offset
        };
    }
}

public sealed record GetTopSongsByCountryQuery(string Country, int limit = 20, int offset = 0) : IQuery<SearchResult<Song>>;
