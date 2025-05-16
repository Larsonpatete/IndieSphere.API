using IndieSphere.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

public class UsersController(IMediator mediator) : ApiControllerBase
{
    private readonly IMediator mediator = mediator;


    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUser(int userId)
    {
        var user = await mediator.Send(new GetUserQuery(userId));
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}
