using IndieSphere.Application.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class SongController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("search")]  // Changed to GET
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 20) {
        var result = await _mediator.Send(new SearchSongsQuery(query,  limit));
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

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetSongDetails(string Id)
    {
        var result = await _mediator.Send(new GetSongDetailsQuery(Id));
        return Ok(result);
    }
}
