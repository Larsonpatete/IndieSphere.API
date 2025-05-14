using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IndieSphere.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}