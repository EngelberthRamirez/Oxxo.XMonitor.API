using ApplicationCore.Features.Update.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet, Route("GetAllUpdates/{storeName}/{storeDataId?}")]
    public async Task<IActionResult> GetAllUpdates(string storeName, int storeDataId = 0)
    {
        var result = await _mediator.Send(new GetAllUpdates.Query
        {
            StoreName = storeName,
            StoreDataId = storeDataId
        });

        return Ok(result);
    }
}
