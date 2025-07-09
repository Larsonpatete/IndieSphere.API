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
            var lastFmArtistTask = _lastFmService.GetArtistInfo(artist.Name);
            var topTracksTask = _lastFmService.GetArtistTopTracks(artist.Name);
            var topAlbumsTask = _lastFmService.GetArtistTopAlbums(artist.Name);
            var similarArtistsTask = _lastFmService.GetSimilarArtists(artist.Name);

            await Task.WhenAll(lastFmArtistTask, topTracksTask, topAlbumsTask, similarArtistsTask);

            var lastFmArtist = await lastFmArtistTask;
            var topTracks = await topTracksTask;
            var topAlbums = await topAlbumsTask;
            var similarArtists = await similarArtistsTask;

            // Populate artist details
            //if (lastFmArtist?.Bio?.Summary is not null)
            //{
            //    artist.Bio = lastFmArtist.Bio.Summary;
            //}

            //artist.TopTracks = topTracks.Select(Song.MapLastFmTopTrackToSong).ToList();
            //artist.TopAlbums = topAlbums.Select(Album.MapLastFmTopAlbumToAlbum).ToList();
            //artist.SimilarArtists = similarArtists.Select(Artist.MapLastFmArtist).ToList();


        }
        catch (Exception e)
        {

        }
        
        return artist;
    }
}
public sealed record GetArtistDetailsQuery(string Id) : IQuery<Artist>;
