namespace SecureGate.Application.Interfaces;

public interface IRateLimiter
{
    /// <summary>
    /// Records the current request and reports whether it stays within <paramref name="limit"/>
    /// requests over the given <paramref name="window"/>. A caller that wants an unlimited plan
    /// (for example Enterprise) must skip this check entirely rather than pass a huge limit.
    /// </summary>
    Task<RateLimitResult> CheckAsync(string clientId, int limit, TimeSpan window);
}
