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
    [HttpGet]
    [Route("reboot")]
    public async Task<IActionResult> RebootCashRegister([FromQuery] RebootCashRegisterCommandParameters queryParams)
    {
        await mediator.Send(new RebootCashRegisterCommand { Parameters = queryParams });
        return Ok();
    }
}
