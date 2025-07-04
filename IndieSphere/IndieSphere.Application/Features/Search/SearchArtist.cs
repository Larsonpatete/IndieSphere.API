using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.Spotify;
using MediatR;


namespace IndieSphere.Application.Features.Search;

public class SearchArtistHandler(ISpotifyService spotifyService) : IRequestHandler<SearchArtistQuery, SearchResult<Artist>>
{
    private readonly ISpotifyService _spotifyService = spotifyService;
    public async Task<SearchResult<Artist>> Handle(SearchArtistQuery request, CancellationToken cancellationToken)
    {
        // Here you would implement the logic to search for artists.
        // For now, let's assume we have a service that does this.
        var result = await _spotifyService.SearchArtistsAsync(request.Query, request.limit, request.offset);
        return result;
    }
}

public sealed record SearchArtistQuery(string Query, int limit, int offset) : IQuery<SearchResult<Artist>>;

