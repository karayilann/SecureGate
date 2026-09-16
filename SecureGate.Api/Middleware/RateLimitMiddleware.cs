using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;

namespace SecureGate.Api.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public RateLimitMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Reads the already-validated key from <c>context.Items</c> (populated by
    /// <see cref="ApiKeyAuthMiddleware"/>) so no extra database call is needed here. Enterprise plans
    /// are treated as unlimited and skip the limiter entirely, which also avoids piling up Redis
    /// entries for a limit that would never be reached.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter)
    {
        if (!context.Request.Path.StartsWithSegments("/proxy"))
        {
            await _next(context);
            return;
        }

        if (context.Items["ApiKey"] is not ApiKey apiKey || apiKey.Plan is null)
        {
            await _next(context);
            return;
        }

        if (apiKey.Plan.Name == PlanType.Enterprise)
        {
            await _next(context);
            return;
        }

        var limit = apiKey.Plan.RequestsPerMinute;
        var result = await rateLimiter.CheckAsync(apiKey.Id.ToString(), limit, Window);

        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(result.Remaining, 0).ToString();

        if (!result.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = result.RetryAfterSeconds.ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "RateLimitExceeded",
                limit,
                windowSeconds = (int)Window.TotalSeconds,
                retryAfterSeconds = result.RetryAfterSeconds
            });

            return;
        }

        await _next(context);
    }
}
