using MediatR;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.ApiKeys.Queries.GetAllApiKeys;

public class GetAllApiKeysQueryHandler : IRequestHandler<GetAllApiKeysQuery, IReadOnlyList<ApiKeyListItemDto>>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public GetAllApiKeysQueryHandler(IApiKeyRepository apiKeyRepository) => _apiKeyRepository = apiKeyRepository;

    public async Task<IReadOnlyList<ApiKeyListItemDto>> Handle(GetAllApiKeysQuery request, CancellationToken cancellationToken)
    {
        var keys = await _apiKeyRepository.GetAllAsync();

        return keys.Select(key => new ApiKeyListItemDto
        {
            Id = key.Id,
            MaskedKeyValue = Mask(key.KeyValue),
            PlanName = key.Plan?.Name.ToString() ?? string.Empty,
            Status = key.Status.ToString(),
            CreatedAt = key.CreatedAt
        }).ToList();
    }

    private static string Mask(string keyValue) =>
        keyValue.Length <= 8 ? new string('*', keyValue.Length) : $"{keyValue[..4]}…{keyValue[^4..]}";
}
