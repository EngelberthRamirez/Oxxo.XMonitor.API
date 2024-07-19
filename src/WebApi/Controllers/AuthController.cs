using ApplicationCore.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public Task<GenerateToken.Response> Token([FromBody] GenerateToken.Request request) =>
            mediator.Send(new GenerateToken.Command { Request = request });

        [HttpGet("isalive")]
        public IActionResult IsAlive()
        {
            return Ok("API is working!");
        }
    }
}
