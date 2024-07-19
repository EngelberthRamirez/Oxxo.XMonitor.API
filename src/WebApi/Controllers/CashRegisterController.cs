using ApplicationCore.Features.CashRegister.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CashRegisterController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Envia al proxy llamar un comando para reiniciar la caja
    /// </summary>
    /// <param name="queryParams"></param>
    /// <returns></returns>
    [HttpGet("reboot")]
    public async Task<IActionResult> RebootCashRegister([FromQuery] RebootCashRegister.Request queryParams)
    {
        await mediator.Send(new RebootCashRegister.Command { Parameters = queryParams });
        return Ok();
    }
}
