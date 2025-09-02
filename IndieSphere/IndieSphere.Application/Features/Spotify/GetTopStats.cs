using IndieSphere.Domain.Music;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Spotify;

public class GetTopStats(ISpotifyService spotifyService) : IRequestHandler<GetTopStatsQuery, GetTopStatsResponse>
{
    private readonly ISpotifyService _spotifyService = spotifyService;
    public async Task<GetTopStatsResponse> Handle(GetTopStatsQuery request, CancellationToken cancellationToken)
    {
        var songs = await _spotifyService.GetUserTopTracksAsync();
        var artists = await _spotifyService.GetUserTopArtistsAsync();
        return new GetTopStatsResponse
        {
            TopTracks = songs,
            TopArtists = artists
        };
    }
}

public sealed record GetTopStatsQuery() : IQuery<GetTopStatsResponse>;

public class GetTopStatsResponse
{
    public IEnumerable<Song> TopTracks { get; set; }
    public IEnumerable<Artist> TopArtists { get; set; }
}
