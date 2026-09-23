using MediatR;
using SecureGate.Application.DTOs;

namespace SecureGate.Application.Features.ApiKeys.Queries.GetAllApiKeys;

public record GetAllApiKeysQuery : IRequest<IReadOnlyList<ApiKeyListItemDto>>;
