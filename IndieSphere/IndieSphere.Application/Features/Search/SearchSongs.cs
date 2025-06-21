using IndieSphere.Domain.Music;
using IndieSphere.Infrastructure.NLP;
using IndieSphere.Infrastructure.Search;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class SearchSongsHandler : IRequestHandler<SearchSongsQuery, MusicQuery>
{
    private readonly INlpService _nlpService;
    private readonly ISearchService _searchService;

    public SearchSongsHandler(INlpService nlpService, ISearchService searchService)
    {
        _nlpService = nlpService;
        _searchService = searchService;
    }

    public async Task<MusicQuery> Handle(SearchSongsQuery request, CancellationToken cancellationToken)
    {
        // Use NLP service, then call MusicBrainz/Last.fm as needed
        //var nlpResult = await _nlpService.AnalyzeTextAsync(request.Query);
        // ...call other services, aggregate results, etc...
        //return nlpResult;
        var result = await _nlpService.AnalyzeQueryAsync(request.Query);

        return result ?? null;
    }
}

public sealed record SearchSongsQuery(string Query, int limit) : IQuery<MusicQuery>;
