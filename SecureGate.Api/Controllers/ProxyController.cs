using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecureGate.Application.Features.Proxy.Queries.ProxyRequest;
using SecureGate.Application.Interfaces;

namespace SecureGate.Api.Controllers;

[ApiController]
[Route("proxy")]
public class ProxyController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProxyController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(BackendResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string resource)
    {
        var result = await _mediator.Send(new ProxyRequestQuery(resource));
        return Ok(result);
    }
}
