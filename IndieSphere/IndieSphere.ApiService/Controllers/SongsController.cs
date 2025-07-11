﻿using IndieSphere.Application.Features.Search;
using IndieSphere.Domain.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class SongsController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("search")]
    public async Task<IActionResult> SearchSongs( // TODO: reconsider NER
    [FromQuery] string query,
    [FromQuery] int limit = 20,
    [FromQuery] int offset = 0
    )
    {
        var result = await _mediator.Send(new SearchSongsQuery(query,  limit, offset));
        return Ok(result);
    }

    [HttpGet("genre")]
    public async Task<IActionResult> GetByGenre([FromQuery] string genre)
    {
        return Ok();
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetSongDetails(string Id) // TODO: handle MbId
    {
        var result = await _mediator.Send(new GetSongDetailsQuery(Id));
        return Ok(result);
    }

    [HttpGet("similar-songs")]
    public async Task<IActionResult> GetSimilarSongs(
        [FromQuery] string query,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
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

        var result = await _mediator.Send(new GetSimilarSongQuery(query, limit, filters));
        return Ok(result);
    }

    [HttpGet("top-songs-by-country")]
    public async Task<IActionResult> GetTopSongsByCountry(
        [FromQuery] string country,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        var result = await _mediator.Send(new GetTopSongsByCountryQuery(country, limit, offset));
        return Ok(result);
    }
}
