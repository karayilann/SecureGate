namespace SecureGate.Application.Interfaces;

public sealed record RateLimitResult(bool IsAllowed, int Limit, int Remaining, int RetryAfterSeconds);
