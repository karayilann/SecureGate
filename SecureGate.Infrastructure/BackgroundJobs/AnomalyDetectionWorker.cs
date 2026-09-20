using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecureGate.Application.Common;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.BackgroundJobs;

public class AnomalyDetectionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnomalyDetectionWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    public AnomalyDetectionWorker(IServiceScopeFactory scopeFactory, ILogger<AnomalyDetectionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanForAnomaliesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Anomaly detection scan failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    /// <summary>
    /// A background service is a singleton, so scoped services (DbContext, repositories, cache) must be
    /// resolved inside a per-iteration scope rather than injected into the constructor.
    /// </summary>
    private async Task ScanForAnomaliesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var usageRepository = provider.GetRequiredService<IUsageRepository>();
        var apiKeyRepository = provider.GetRequiredService<IApiKeyRepository>();
        var anomalyLogRepository = provider.GetRequiredService<IAnomalyLogRepository>();
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var cacheService = provider.GetRequiredService<ICacheService>();
        var detector = provider.GetRequiredService<IAnomalyDetector>();

        var since = DateTime.UtcNow - Window;
        var records = await usageRepository.GetRecentAsync(since);
        var anomalies = detector.Detect(records);

        foreach (var anomaly in anomalies)
        {
            var apiKey = await apiKeyRepository.GetByIdAsync(anomaly.ApiKeyId);
            if (apiKey is null || apiKey.Status == KeyStatus.Suspended)
            {
                continue;
            }

            apiKey.Status = KeyStatus.Suspended;
            await apiKeyRepository.UpdateAsync(apiKey);

            await anomalyLogRepository.AddAsync(new AnomalyLog
            {
                ApiKeyId = apiKey.Id,
                Reason = anomaly.Reason,
                DistinctIpCount = anomaly.DistinctIpCount
            });

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync(CacheKeys.ApiKey(apiKey.KeyValue), cancellationToken);

            _logger.LogWarning("API key {ApiKeyId} suspended by anomaly detection: {Reason}", apiKey.Id, anomaly.Reason);
        }
    }
}
