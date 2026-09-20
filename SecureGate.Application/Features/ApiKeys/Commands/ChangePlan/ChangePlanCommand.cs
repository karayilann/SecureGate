using MediatR;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Enums;

namespace SecureGate.Application.Features.ApiKeys.Commands.ChangePlan;

public record ChangePlanCommand(Guid ApiKeyId, PlanType PlanType) : IRequest<ApiKeyDto?>;
