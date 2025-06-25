using IndieSphere.Domain.Music;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.Search;
using IndieSphere.Infrastructure.Spotify;
using MediatR;

namespace IndieSphere.Application.Features.Search;

public class GetSongDetailsHandler(ISearchService searchService, ISpotifyService spotifyService, ILastFmService lastFmService) : IRequestHandler<GetSongDetailsQuery, SongResult>
{
    private readonly ISearchService _searchService = searchService;
    private readonly ISpotifyService _spotifyService = spotifyService;
    private readonly ILastFmService _lastFmService = lastFmService;
    public async Task<SongResult> Handle(GetSongDetailsQuery request, CancellationToken cancellationToken)
    {
        var song = await _spotifyService.GetSongDetailsAsync(request.Id);
        if (song == null)
        {
            return null;
        }

        try
        {
            // Get Last.fm data
            var lastFmTrack = await _lastFmService.GetTrackInfo(song.Artist.Name, song.Title);

            // Enrich song with Last.fm data if available
            if (lastFmTrack?.Track != null)
            {
                // Add play count and listener count
                song.PlayCount = lastFmTrack.Track.Playcount;
                song.ListenerCount = lastFmTrack.Track.Listeners;

                // Add Last.fm description/wiki content if available
                if (lastFmTrack.Track.Wiki != null)
                {
                    song.Description = lastFmTrack.Track.Wiki.Content;
                }

                // Add user tags if available
                if (lastFmTrack.Track.TopTags?.Tags != null && lastFmTrack.Track.TopTags.Tags.Any())
                {
                    // Either create new Tag objects
                    var userTags = lastFmTrack.Track.TopTags.Tags
                        .Select(t => new Tag { Name = t.Name })
                        .ToList();

                    // Add them to existing genres if song.UserTags property exists
                    // or add to genres if that's where you store tags
                    if (song.Genres == null)
                    {
                        song.Genres = new List<Genre>();
                    }

                    // Add Last.fm tags as genres if they don't already exist
                    foreach (var tag in lastFmTrack.Track.TopTags.Tags)
                    {
                        if (!song.Genres.Any(g => g.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            song.Genres.Add(new Genre { Name = tag.Name });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but continue - don't fail if Last.fm is unavailable
            Console.WriteLine($"Error getting Last.fm data: {ex.Message}");
        }

        // Return the enriched song
        return new SongResult(song);
    }

}
public sealed record GetSongDetailsQuery(string Id) : IQuery<SongResult>; // MusicQuery

