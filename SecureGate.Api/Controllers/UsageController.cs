using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureGate.Application.DTOs;
using SecureGate.Application.Features.Usage.Queries.GetUsageStats;

namespace SecureGate.Api.Controllers;

[ApiController]
[Route("api/admin/usage")]
[Authorize(Roles = "Admin")]
public class UsageController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsageController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(UsageStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetUsageStatsQuery());
        return Ok(result);
    }
}
