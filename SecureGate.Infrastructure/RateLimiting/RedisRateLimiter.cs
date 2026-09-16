using SecureGate.Application.Interfaces;
using StackExchange.Redis;

namespace SecureGate.Infrastructure.RateLimiting;

public sealed class RedisRateLimiter : IRateLimiter
{
    private const string KeyPrefix = "ratelimit:";

    private readonly IDatabase _db;

    public RedisRateLimiter(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    /// <summary>
    /// Runs the whole prune-count-add sequence inside one Lua script so concurrent requests cannot
    /// race between counting and recording. The retry-after value is derived from when the oldest
    /// request in the window will expire, which is the earliest moment a new request could succeed.
    /// </summary>
    public async Task<RateLimitResult> CheckAsync(string clientId, int limit, TimeSpan window)
    {
        var key = KeyPrefix + clientId;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowMs = (long)window.TotalMilliseconds;
        var member = $"{now}-{Guid.NewGuid():N}";

        var raw = (RedisResult[])(await _db.ScriptEvaluateAsync(
            RateLimiterScripts.SlidingWindow,
            new RedisKey[] { key },
            new RedisValue[] { now, windowMs, limit, member }))!;

        var isAllowed = (long)raw[0] == 1;
        var remaining = (int)(long)raw[1];
        var retryMs = (long)raw[2];
        var retryAfterSeconds = retryMs > 0 ? (int)Math.Ceiling(retryMs / 1000.0) : 0;

        return new RateLimitResult(isAllowed, limit, remaining, retryAfterSeconds);
    }
}
