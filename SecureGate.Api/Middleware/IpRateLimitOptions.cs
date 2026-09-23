namespace SecureGate.Api.Middleware;

public class IpRateLimitOptions
{
    public int RequestsPerMinute { get; set; } = 100;
}
