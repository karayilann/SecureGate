using MediatR;
using SecureGate.Application.DTOs;

namespace SecureGate.Application.Features.ApiKeys.Commands.ActivateKey;

public record ActivateKeyCommand(Guid ApiKeyId) : IRequest<ApiKeyDto?>;
