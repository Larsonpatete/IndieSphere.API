using IndieSphere.Application.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class SongController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        var result = await _mediator.Send(new SearchSongsQuery(request.Query, request.limit));
        //var nlpResult = await _nlpService.AnalyzeTextAsync(query);
        // Limit the number of songs returned
        //var limitedSongs = songs.Take(limit).ToList();
        return Ok(result);
    }

    public class SearchRequest
    {
        public string Query { get; set; }
        public int limit { get; set; } = 10;
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
