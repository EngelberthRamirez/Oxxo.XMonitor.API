using ApplicationCore.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<TokenCommandResponse> Token([FromBody] TokenCommand command) =>
            _mediator.Send(command);

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("API is working!");
        }
    }
}
