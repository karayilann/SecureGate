using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecureGate.Application.DTOs;
using SecureGate.Application.Features.ApiKeys.Commands.CreateApiKey;
using SecureGate.Application.Features.ApiKeys.Queries.GetApiKeyById;

namespace SecureGate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KeysController : ControllerBase
{
    private readonly IMediator _mediator;

    public KeysController(IMediator mediator) => _mediator = mediator;

    
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetApiKeyByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }
}