using IndieSphere.Domain.Helper;
using IndieSphere.Domain.Music;
using IndieSphere.Domain.Search;
using IndieSphere.Infrastructure.LastFm;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class GetSimilarSongHandler(ILastFmService lastFm) : IRequestHandler<GetSimilarSongQuery, SearchResult<Song>>
{
    private readonly ILastFmService _lastFmService = lastFm;  
    public async Task<SearchResult<Song>> Handle(GetSimilarSongQuery request, CancellationToken cancellationToken)
    {
        var parsed = SongQueryParser.Parse(request.Query);
        if (parsed == null)
            throw new ArgumentException("Query must be in the format '[song] by [artist]'.");

        var result = await _lastFmService.GetSimilarSongs(parsed.Value.Song, parsed.Value.Artist, request.limit);
        return new SearchResult<Song> { Results = result.Select(Song.MapLastFmTrackToSong).ToList(), Limit = request.limit, TotalCount = result.Count() };
    }
}

public sealed record GetSimilarSongQuery(string Query, int limit) : IQuery<SearchResult<Song>>; // MusicQuery

