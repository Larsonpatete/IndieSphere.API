using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Search;
public class GetArtistDetailsHandler(ISpotifyService spotifyService, ILastFmService lastFmService) : IRequestHandler<GetArtistDetailsQuery, Artist>
{
    private readonly ISpotifyService _spotifyService = spotifyService;
    private readonly ILastFmService _lastFmService = lastFmService;
    public async Task<Artist> Handle(GetArtistDetailsQuery request, CancellationToken cancellationToken)
    {
        var artist = await _spotifyService.GetArtistDetailsAsync(request.Id);
        return artist;
    }
}
public sealed record GetArtistDetailsQuery(string Id) : IQuery<Artist>;
