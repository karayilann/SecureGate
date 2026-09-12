using MediatR;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Enums;

namespace SecureGate.Application.Features.ApiKeys.Commands.CreateApiKey;

public record CreateApiKeyCommand(Guid UserId, PlanType PlanType) : IRequest<ApiKeyDto>;