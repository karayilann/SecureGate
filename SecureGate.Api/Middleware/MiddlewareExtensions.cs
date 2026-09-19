namespace SecureGate.Api.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the SecureGate request pipeline in order: global error handling first so it wraps the
    /// rest, then API-key authentication, then rate limiting (which trusts the key the auth step stored),
    /// then usage logging (which records only requests that passed both).
    /// </summary>
    public static IApplicationBuilder UseSecureGatePipeline(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<ApiKeyAuthMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();
        app.UseMiddleware<UsageLoggingMiddleware>();

        return app;
    }
}
