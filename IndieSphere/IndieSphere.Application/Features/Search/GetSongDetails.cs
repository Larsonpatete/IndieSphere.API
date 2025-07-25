﻿using IndieSphere.Domain.Music;
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
                song.EnrichWithLastFm(lastFmTrack.Track);
            }
        }
        catch (Exception ex)
        {
            // Log the error but continue - don't fail if Last.fm is unavailable
            Console.WriteLine($"Error getting Last.fm data: {ex.Message}");
        }
        var lastFmSongs = await _lastFmService.GetSimilarSongs(song.Title, song.Artist.Name, 9); // 9 fits better in UI lol
        var similarSongs = lastFmSongs.Select(Song.MapLastFmTrackToSong).ToList();
        await _spotifyService.EnrichWithSpotify(similarSongs);
        var albumSongs = await _spotifyService.GetAlbumSongs(song.Album.Id);
        song.Album.Songs = albumSongs;
        song.SimilarSongs = similarSongs;

        // Return the enriched song
        return new SongResult(song);
    }

}
public sealed record GetSongDetailsQuery(string Id) : IQuery<SongResult>; // MusicQuery


public sealed record SongResult(Song Song);

