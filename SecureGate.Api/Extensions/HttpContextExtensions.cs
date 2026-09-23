namespace SecureGate.Api.Extensions;

public static class HttpContextExtensions
{
    private const string ForwardedForHeader = "X-Forwarded-For";

    /// <summary>
    /// Resolves the caller's IP from X-Forwarded-For first (the load test and real deployments sit behind a
    /// proxy) and falls back to the socket address.
    /// </summary>
    public static string GetClientIp(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ForwardedForHeader, out var forwarded) &&
            !string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.ToString().Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
