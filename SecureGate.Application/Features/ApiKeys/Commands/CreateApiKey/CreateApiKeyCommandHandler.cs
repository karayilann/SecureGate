using AutoMapper;
using MediatR;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, ApiKeyDto>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateApiKeyCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _apiKeyRepository = apiKeyRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiKeyDto> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByTypeAsync(request.PlanType)
            ?? throw new InvalidOperationException($"'{request.PlanType}' planı bulunamadı");

        var apiKey = new ApiKey
        {
            UserId = request.UserId,
            PlanId = plan.Id,
            Plan = plan,
            KeyValue = GenerateKeyValue()
        };

        await _apiKeyRepository.AddAsync(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ApiKeyDto>(apiKey);
    }

    private static string GenerateKeyValue() =>
        Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "").Replace("+", "").Replace("/", "");
}