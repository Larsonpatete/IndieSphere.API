using IndieSphere.Domain.LastFm;
using IndieSphere.Domain.Music;
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
        if (artist == null) return null;

        try
        {
            var lastFmArtist = await _lastFmService.GetArtistInfo(artist.Name);
            //IEnumerable<LastFmTopTrack> topTracks = await _lastFmService.GetArtistTopTracks(artist.Name);
            IEnumerable<LastFmTopAlbum> topAlbums = await _lastFmService.GetArtistTopAlbums(artist.Name);
            var similarArtists = await _lastFmService.GetSimilarArtists(artist.Name);

            //await Task.WhenAll(lastFmArtistTask, topTracksTask, topAlbumsTask, similarArtistsTask);

            //var lastFmArtist =  lastFmArtistTask;
            //var topTracks =  topTracksTask;
            //var topAlbums =  topAlbumsTask;
            //var similarArtists =  similarArtistsTask;

            // Populate artist details
            //if (lastFmArtist?.Bio?.Summary is not null)
            //{
            //    artist.Bio = lastFmArtist.Bio.Summary;
            //}

            IEnumerable<LastFmTopTrack> lastFmTopTracks = await _lastFmService.GetArtistTopTracks(artist.Name);
            IEnumerable<TopTrack> topTracks = lastFmTopTracks.Select(TopTrack.MapLastFmTopTrackToTopTrack);

            artist.TopTracks = topTracks.Select(Song.MapLastFmTopTrackToSong).ToList();
            artist.TopAlbums = topAlbums.Select(Album.MapLastFmTopAlbumToAlbum).ToList();
            artist.SimilarArtists = similarArtists.Select(Artist.MapLastFmArtist).ToList();


        }
        catch (Exception e)
        {

        }
        
        return artist;
    }
}
public sealed record GetArtistDetailsQuery(string Id) : IQuery<Artist>;
