using IndieSphere.Application.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class SongController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        var result = await _mediator.Send(new SearchSongsQuery(query, limit));
        //var nlpResult = await _nlpService.AnalyzeTextAsync(query);
        // Limit the number of songs returned
        //var limitedSongs = songs.Take(limit).ToList();
        return Ok(result);
    }

    [HttpGet("artist")]
    public async Task<IActionResult> GetArtist([FromQuery] string name)
    {
        return Ok();
    }


    [HttpGet("genre")]
    public async Task<IActionResult> GetByGenre([FromQuery] string genre)
    {
        return Ok();
    }

    [HttpGet("song")]
    public async Task<IActionResult> GetSong([FromQuery] string title, [FromQuery] string artist)
    {
        return Ok();
    }
}
