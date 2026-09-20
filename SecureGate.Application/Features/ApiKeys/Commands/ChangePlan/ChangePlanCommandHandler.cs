using AutoMapper;
using MediatR;
using SecureGate.Application.Common;
using SecureGate.Application.DTOs;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.ApiKeys.Commands.ChangePlan;

public class ChangePlanCommandHandler : IRequestHandler<ChangePlanCommand, ApiKeyDto?>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public ChangePlanCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IMapper mapper)
    {
        _apiKeyRepository = apiKeyRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ApiKeyDto?> Handle(ChangePlanCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(request.ApiKeyId);
        if (apiKey is null)
        {
            return null;
        }

        var plan = await _planRepository.GetByTypeAsync(request.PlanType)
            ?? throw new InvalidOperationException($"'{request.PlanType}' plani bulunamadi");

        apiKey.PlanId = plan.Id;
        apiKey.Plan = plan;
        await _apiKeyRepository.UpdateAsync(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.ApiKey(apiKey.KeyValue), cancellationToken);

        return _mapper.Map<ApiKeyDto>(apiKey);
    }
}
