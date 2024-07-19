using ApplicationCore.Features.Auditoria.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditoriaController(IMediator mediator) : ControllerBase
{
    [HttpGet("dsc/DscGetLastAudithByStoreName/{storeName}")]
    public async Task<IActionResult> DscGetLastAudithByStoreName(string storeName)
    {
        var result = await mediator.Send(new GetLastAudithByStoreName.Query
        {
            Request = new GetLastAudithByStoreName.Request { StoreName = storeName }
        });
        return Ok(result);
    }

    [HttpGet("dsc/GetDSCAudithListPatches")]
    public async Task<IActionResult> GetDSCAudithListPatches()
    {
        var result = await mediator.Send(new GetDSCAudithListPatches.Query());
        return Ok(result);
    }

    [HttpGet("dsc/GetDscFullConfig")]
    public async Task<IActionResult> GetDscFullConfig()
    {
        var result = await mediator.Send(new GetDscFullConfig.Query());
        return Ok(result);
    }
}
