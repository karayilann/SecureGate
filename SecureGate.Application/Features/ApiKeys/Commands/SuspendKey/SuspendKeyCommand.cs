using MediatR;
using SecureGate.Application.DTOs;

namespace SecureGate.Application.Features.ApiKeys.Commands.SuspendKey;

public record SuspendKeyCommand(Guid ApiKeyId) : IRequest<ApiKeyDto?>;
