using System.Diagnostics;
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
        var stopwatch = Stopwatch.StartNew();
        var result = await _mediator.Send(new ProxyRequestQuery(resource));
        stopwatch.Stop();

        Response.Headers["X-Cache"] = result.FromCache ? "HIT" : "MISS";
        Response.Headers["X-Response-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();

        return Ok(result.Response);
    }
}
