using IndieSphere.Infrastructure.NLP;
using IndieSphere.Infrastructure.Search;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class SearchSongsHandler : IRequestHandler<SearchSongsQuery, NlpResult>
{
    private readonly INlpService _nlpService;
    private readonly ISearchService _searchService;

    public SearchSongsHandler(INlpService nlpService, ISearchService searchService)
    {
        _nlpService = nlpService;
        _searchService = searchService;
    }

    public async Task<NlpResult> Handle(SearchSongsQuery request, CancellationToken cancellationToken)
    {
        // Use NLP service, then call MusicBrainz/Last.fm as needed
        var nlpResult = await _nlpService.AnalyzeTextAsync(request.Query);
        // ...call other services, aggregate results, etc...
        return nlpResult;
    }
}

public sealed record SearchSongsQuery(string Query, int limit) : IQuery<NlpResult>;
