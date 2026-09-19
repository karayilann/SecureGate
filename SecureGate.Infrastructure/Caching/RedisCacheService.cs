using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SecureGate.Application.Interfaces;

namespace SecureGate.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// The cache is a speed-up, not a source of truth, so a read never fails the request: if Redis is
    /// unreachable the caller treats it as a miss and goes to the backend (graceful degradation).
    /// Cancellation is left to propagate so it is not mistaken for a cache outage.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);
            return json is null ? default : JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache read failed for key {CacheKey}; falling back to the backend.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
            await _cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache write failed for key {CacheKey}; continuing without caching.", key);
        }
    }
}
