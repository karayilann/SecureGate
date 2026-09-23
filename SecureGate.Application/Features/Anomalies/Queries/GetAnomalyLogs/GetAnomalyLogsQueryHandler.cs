using MediatR;
using SecureGate.Application.Common;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.Anomalies.Queries.GetAnomalyLogs;

public class GetAnomalyLogsQueryHandler : IRequestHandler<GetAnomalyLogsQuery, IReadOnlyList<AnomalyLogDto>>
{
    private const int MaxItems = 100;
    private readonly IAnomalyLogRepository _anomalyLogRepository;

    public GetAnomalyLogsQueryHandler(IAnomalyLogRepository anomalyLogRepository) =>
        _anomalyLogRepository = anomalyLogRepository;

    public async Task<IReadOnlyList<AnomalyLogDto>> Handle(GetAnomalyLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _anomalyLogRepository.GetRecentAsync(MaxItems);

        return logs.Select(log => new AnomalyLogDto
        {
            Id = log.Id,
            ApiKeyId = log.ApiKeyId,
            MaskedKeyValue = log.ApiKey is null ? string.Empty : KeyMasking.Mask(log.ApiKey.KeyValue),
            Reason = log.Reason,
            DistinctIpCount = log.DistinctIpCount,
            DetectedAt = log.DetectedAt
        }).ToList();
    }
}
