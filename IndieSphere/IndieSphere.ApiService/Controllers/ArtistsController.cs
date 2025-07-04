using IndieSphere.Application.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class ArtistsController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("search")]
    public async Task<IActionResult> GetArtist(
    [FromQuery] string query,
    [FromQuery] int limit = 20,
    [FromQuery] int offset = 0
    )
    {
        var result = await _mediator.Send(new SearchArtistQuery(query, limit, offset));
        return Ok(result);
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetArtistDetails(string Id)
    {
        var result = await _mediator.Send(new GetArtistDetailsQuery(Id));
        return Ok(result);
    }
}
