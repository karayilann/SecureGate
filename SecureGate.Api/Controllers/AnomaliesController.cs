using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureGate.Application.DTOs;
using SecureGate.Application.Features.Anomalies.Queries.GetAnomalyLogs;

namespace SecureGate.Api.Controllers;

[ApiController]
[Route("api/admin/anomalies")]
[Authorize(Roles = "Admin")]
public class AnomaliesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnomaliesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AnomalyLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAnomalyLogsQuery());
        return Ok(result);
    }
}
