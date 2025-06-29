﻿using IndieSphere.Application.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class SearchController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("songs")] 
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 20, [FromQuery] int offset = 0) { // TODO: reconsider NER
        var result = await _mediator.Send(new SearchSongsQuery(query,  limit, offset));
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
    public async Task<IActionResult> GetSongDetails(string Id) // TODO: handle MbId
    {
        var result = await _mediator.Send(new GetSongDetailsQuery(Id));
        return Ok(result);
    }

    [HttpGet("similar-songs")]
    public async Task<IActionResult> GetSimilarSongs([FromQuery] string query, [FromQuery] int limit = 20)
    {
        var result = await _mediator.Send(new GetSimilarSongQuery(query, limit));
        return Ok(result);
    }
}
