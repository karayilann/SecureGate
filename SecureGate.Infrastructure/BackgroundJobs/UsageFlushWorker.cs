using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.BackgroundJobs;

public class UsageFlushWorker : BackgroundService
{
    private const int BatchSize = 100;
    private readonly UsageQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UsageFlushWorker> _logger;

    public UsageFlushWorker(UsageQueue queue, IServiceScopeFactory scopeFactory, ILogger<UsageFlushWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = await ReadBatchAsync(stoppingToken);
                await FlushAsync(batch, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Usage flush failed.");
            }
        }
    }

    /// <summary>
    /// Blocks until at least one record is queued, then drains whatever else is already waiting up to the
    /// batch size. Under load this coalesces many records into one write; under light load it writes promptly.
    /// </summary>
    private async Task<List<UsageRecord>> ReadBatchAsync(CancellationToken cancellationToken)
    {
        var reader = _queue.Reader;
        var batch = new List<UsageRecord>(BatchSize)
        {
            await reader.ReadAsync(cancellationToken)
        };

        while (batch.Count < BatchSize && reader.TryRead(out var next))
        {
            batch.Add(next);
        }

        return batch;
    }

    private async Task FlushAsync(List<UsageRecord> batch, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var usageRepository = scope.ServiceProvider.GetRequiredService<IUsageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        foreach (var record in batch)
        {
            await usageRepository.AddAsync(record);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
