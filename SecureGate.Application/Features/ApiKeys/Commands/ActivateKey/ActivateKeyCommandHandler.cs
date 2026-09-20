using AutoMapper;
using MediatR;
using SecureGate.Application.Common;
using SecureGate.Application.DTOs;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Enums;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.ApiKeys.Commands.ActivateKey;

public class ActivateKeyCommandHandler : IRequestHandler<ActivateKeyCommand, ApiKeyDto?>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public ActivateKeyCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IMapper mapper)
    {
        _apiKeyRepository = apiKeyRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ApiKeyDto?> Handle(ActivateKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(request.ApiKeyId);
        if (apiKey is null)
        {
            return null;
        }

        apiKey.Status = KeyStatus.Active;
        await _apiKeyRepository.UpdateAsync(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.ApiKey(apiKey.KeyValue), cancellationToken);

        return _mapper.Map<ApiKeyDto>(apiKey);
    }
}
