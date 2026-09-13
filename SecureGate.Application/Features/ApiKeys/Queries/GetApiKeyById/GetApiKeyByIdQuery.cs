using MediatR;
using SecureGate.Application.DTOs;

namespace SecureGate.Application.Features.ApiKeys.Queries.GetApiKeyById;

public record GetApiKeyByIdQuery(Guid Id) : IRequest<ApiKeyDto?>;