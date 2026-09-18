namespace SecureGate.Api.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the SecureGate request pipeline in order: global error handling first so it wraps the
    /// rest, then API-key authentication, then rate limiting (which trusts the key the auth step stored).
    /// </summary>
    public static IApplicationBuilder UseSecureGatePipeline(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<ApiKeyAuthMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();

        return app;
    }
}
