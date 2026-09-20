using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureGate.Application.DTOs;
using SecureGate.Application.Features.ApiKeys.Commands.ActivateKey;
using SecureGate.Application.Features.ApiKeys.Commands.ChangePlan;
using SecureGate.Application.Features.ApiKeys.Commands.SuspendKey;
using SecureGate.Domain.Enums;

namespace SecureGate.Api.Controllers;

[ApiController]
[Route("api/admin/keys")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpPatch("{id:guid}/plan")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePlan(Guid id, [FromBody] ChangePlanRequest request)
    {
        var result = await _mediator.Send(new ChangePlanCommand(id, request.PlanType));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/suspend")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var result = await _mediator.Send(new SuspendKeyCommand(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _mediator.Send(new ActivateKeyCommand(id));
        return result is null ? NotFound() : Ok(result);
    }
}

public record ChangePlanRequest(PlanType PlanType);
