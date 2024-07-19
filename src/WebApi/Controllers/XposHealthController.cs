using ApplicationCore.Features.XposHealth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class XposHealthController(IMediator mediator) : ControllerBase
{
    [HttpGet("configtowatch")]
    public async Task<IActionResult> GetConfigToWatch()
    {
        try
        {
            var result = await mediator.Send(new GetConfigToWatch.Query());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
