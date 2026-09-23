using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureGate.Application.Anomaly;
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
    private readonly TimeSpan _interval;
    private readonly TimeSpan _window;
    private readonly int _distinctIpThreshold;

    public AnomalyDetectionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AnomalyDetectionWorker> logger,
        IOptions<AnomalyOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(options.Value.ScanIntervalSeconds);
        _window = TimeSpan.FromMinutes(options.Value.WindowMinutes);
        _distinctIpThreshold = options.Value.DistinctIpThreshold;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanForAnomaliesAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Anomaly detection scan failed.");
            }
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

        var since = DateTime.UtcNow - _window;
        var records = await usageRepository.GetRecentAsync(since);
        var anomalies = detector.Detect(records, _distinctIpThreshold);

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
