using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Api.Middleware;

public class UsageLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private const string ForwardedForHeader = "X-Forwarded-For";

    public UsageLoggingMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Records one usage row per authorized proxy request. It runs after auth and rate limiting, so only
    /// requests that passed both are logged. The IP is taken from X-Forwarded-For first (the load test and
    /// real deployments sit behind a proxy) and falls back to the socket address, which is what the anomaly
    /// worker later scans for "too many distinct IPs on one key".
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IUsageRepository usageRepository, IUnitOfWork unitOfWork)
    {
        await _next(context);

        if (context.Request.Path.StartsWithSegments("/proxy") &&
            context.Response.StatusCode == StatusCodes.Status200OK &&
            context.Items["ApiKey"] is CachedApiKey apiKey)
        {
            var usage = new UsageRecord
            {
                ApiKeyId = apiKey.Id,
                IpAddress = ResolveIpAddress(context),
                Endpoint = context.Request.Path
            };

            await usageRepository.AddAsync(usage);
            await unitOfWork.SaveChangesAsync(context.RequestAborted);
        }
    }

    private static string ResolveIpAddress(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ForwardedForHeader, out var forwarded) &&
            !string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.ToString().Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
