using IndieSphere.Domain.Music;
using IndieSphere.Infrastructure.NLP;
using IndieSphere.Infrastructure.Search;
using IndieSphere.Infrastructure.Spotify;
using MediatR;
using static IndieSphere.Infrastructure.Spotify.SpotifyService;

namespace IndieSphere.Application.Features.Search;

public class SearchSongsHandler : IRequestHandler<SearchSongsQuery, SpotifySearchResult<Song>>
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

    public async Task<SpotifySearchResult<Song>> Handle(SearchSongsQuery request, CancellationToken cancellationToken)
    {
        // Use NLP service, then call MusicBrainz/Last.fm as needed
        //var nlpResult = await _nlpService.AnalyzeTextAsync(request.Query);
        // ...call other services, aggregate results, etc...
        //return nlpResult;

        //var result = await _nlpService.AnalyzeQueryAsync(request.Query);
        // var result = await _searchService.SearchSongsAsync(request.Query, request.limit);
        var result = await _spotifyService.SearchSongsAsync(request.Query, request.limit);

        return result;    
    }

}

public sealed record SearchSongsQuery(string Query, int limit) : IQuery<SpotifySearchResult<Song>>; // MusicQuery
