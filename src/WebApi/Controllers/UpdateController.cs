using ApplicationCore.Features.Update.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UpdateController(IMediator mediator) : ControllerBase
{
    [HttpGet, Route("GetAllUpdates/{storeName}/{storeDataId?}")]
    public async Task<IActionResult> GetAllUpdates(string storeName, int storeDataId = 0)
    {
        var result = await mediator.Send(new GetAllUpdates.Query
        {
            StoreName = storeName,
            StoreDataId = storeDataId
        });

        return Ok(result);
    }
}
