using Microsoft.Extensions.Options;
using SecureGate.Api.Extensions;
using SecureGate.Application.Interfaces;

namespace SecureGate.Api.Middleware;

public class IpRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _requestsPerMinute;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public IpRateLimitMiddleware(RequestDelegate next, IOptions<IpRateLimitOptions> options)
    {
        _next = next;
        _requestsPerMinute = options.Value.RequestsPerMinute;
    }

    /// <summary>
    /// Throttles by client IP before the key is even looked up, so a flood of requests carrying many
    /// different bogus keys from one source is stopped at the edge instead of each distinct key reaching SQL
    /// (cache penetration). Only /proxy is guarded, matching the rest of the pipeline.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter)
    {
        if (!context.Request.Path.StartsWithSegments("/proxy"))
        {
            await _next(context);
            return;
        }

        var ip = context.GetClientIp();
        var result = await rateLimiter.CheckAsync($"ip:{ip}", _requestsPerMinute, Window);

        if (!result.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = result.RetryAfterSeconds.ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "IpRateLimitExceeded",
                limit = _requestsPerMinute,
                windowSeconds = (int)Window.TotalSeconds,
                retryAfterSeconds = result.RetryAfterSeconds
            });

            return;
        }

        await _next(context);
    }
}
