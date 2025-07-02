using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.NLP;
using IndieSphere.Infrastructure.Search;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class SearchSongsHandler : IRequestHandler<SearchSongsQuery, SearchResult<Song>>
{
    private readonly INlpService _nlpService;
    private readonly ISearchService _searchService;
    private readonly ISpotifyService _spotifyService;

    public SearchSongsHandler(INlpService nlpService, ISearchService searchService, ISpotifyService spotify)
    {
        _nlpService = nlpService;
        _searchService = searchService;
        _spotifyService = spotify;
    }

    public async Task<SearchResult<Song>> Handle(SearchSongsQuery request, CancellationToken cancellationToken)
    {
        var result = await _spotifyService.SearchSongsAsync(request.Query, request.limit, request.offset);
        return result;
    }
}

public sealed record SearchSongsQuery(string Query, int limit, int offset) : IQuery<SearchResult<Song>>;
