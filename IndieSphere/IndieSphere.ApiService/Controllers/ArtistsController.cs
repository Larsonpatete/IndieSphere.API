using IndieSphere.Application.Features.Search;
using IndieSphere.Domain.Search;
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

    [HttpGet("similar")]
    public async Task<IActionResult> GetSimilarArtist(
        [FromQuery] string query,
        [FromQuery] int limit = 20,
        [FromQuery] int? minPopularity = null,
        [FromQuery] int? maxPopularity = null)
    {
        // Create filter object
        var filters = new SearchFilters
        {
            MinPopularity = minPopularity,
            MaxPopularity = maxPopularity
            // Add more filters as they're implemented
        };
        var result = await _mediator.Send(new GetSimilarArtistQuery(query, limit, filters));
        return Ok(result);
    }
}
