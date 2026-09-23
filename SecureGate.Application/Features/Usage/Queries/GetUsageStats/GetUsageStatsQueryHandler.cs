using MediatR;
using SecureGate.Application.Common;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Application.Features.Usage.Queries.GetUsageStats;

public class GetUsageStatsQueryHandler : IRequestHandler<GetUsageStatsQuery, UsageStatsDto>
{
    private static readonly TimeSpan Window = TimeSpan.FromHours(24);
    private readonly IUsageRepository _usageRepository;
    private readonly IApiKeyRepository _apiKeyRepository;

    public GetUsageStatsQueryHandler(IUsageRepository usageRepository, IApiKeyRepository apiKeyRepository)
    {
        _usageRepository = usageRepository;
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<UsageStatsDto> Handle(GetUsageStatsQuery request, CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow - Window;
        var records = await _usageRepository.GetRecentAsync(since);
        var keys = await _apiKeyRepository.GetAllAsync();
        var maskById = keys.ToDictionary(k => k.Id, k => KeyMasking.Mask(k.KeyValue));

        var perKey = records
            .GroupBy(r => r.ApiKeyId)
            .Select(g => new UsageByKeyDto
            {
                ApiKeyId = g.Key,
                MaskedKeyValue = maskById.GetValueOrDefault(g.Key, string.Empty),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var timeline = records
            .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, 0, 0, DateTimeKind.Utc))
            .Select(g => new UsageTimelinePointDto { Bucket = g.Key, Count = g.Count() })
            .OrderBy(x => x.Bucket)
            .ToList();

        return new UsageStatsDto { PerKey = perKey, Timeline = timeline };
    }
}
