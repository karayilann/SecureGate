using SecureGate.Api.Extensions;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;

namespace SecureGate.Api.Middleware;

public class UsageLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UsageLoggingMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Records one usage row per successful proxy request. It runs after auth and rate limiting, so only
    /// requests that passed both are logged. The record is handed to an in-memory queue and persisted by a
    /// background flush worker, keeping the SQL write off the request's hot path.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IUsageQueue usageQueue)
    {
        await _next(context);

        if (context.Request.Path.StartsWithSegments("/proxy") &&
            context.Response.StatusCode == StatusCodes.Status200OK &&
            context.Items["ApiKey"] is CachedApiKey apiKey)
        {
            usageQueue.TryEnqueue(new UsageRecord
            {
                ApiKeyId = apiKey.Id,
                IpAddress = context.GetClientIp(),
                Endpoint = context.Request.Path
            });
        }
    }
}
