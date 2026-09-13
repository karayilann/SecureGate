using AutoMapper;
using MediatR;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.ApiKeys.Queries.GetApiKeyById;

public class GetApiKeyByIdQueryHandler : IRequestHandler<GetApiKeyByIdQuery, ApiKeyDto?>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IMapper _mapper;

    public GetApiKeyByIdQueryHandler(IApiKeyRepository apiKeyRepository, IMapper mapper)
    {
        _apiKeyRepository = apiKeyRepository;
        _mapper = mapper;
    }

    public async Task<ApiKeyDto?> Handle(GetApiKeyByIdQuery request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(request.Id);
        return apiKey is null ? null : _mapper.Map<ApiKeyDto>(apiKey);
    }
}