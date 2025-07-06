using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class GetSimilarArtistHandler(ILastFmService lastFm, ISpotifyService spotify) : IRequestHandler<GetSimilarArtistQuery, SearchResult<Artist>>
{
    private readonly ILastFmService _lastFmService = lastFm;
    private readonly ISpotifyService _spotifyService = spotify;
    public async Task<SearchResult<Artist>> Handle(GetSimilarArtistQuery request, CancellationToken cancellationToken)
    {
        var similarArtists = await _lastFmService.GetSimilarArtists(request.Query, request.Limit);
        // Map to Artist objects
        var artists = similarArtists.Select(Artist.MapLastFmArtist).ToList();
        // Populate artist images using Spotify
        await _spotifyService.EnrichArtistWithSpotify(artists);
        artists = artists.ApplyFilters(request.Filters).ToList();
        return new SearchResult<Artist>
        {
            Results = artists,
            Limit = request.Limit,
            TotalCount = artists.Count
        };
    }
}

public sealed record GetSimilarArtistQuery(string Query, int Limit = 20, SearchFilters Filters = null) : IQuery<SearchResult<Artist>>;
