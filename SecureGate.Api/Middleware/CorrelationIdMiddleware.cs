using Serilog.Context;

namespace SecureGate.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Gives every request a correlation id (reusing an inbound X-Correlation-Id if the caller sent one),
    /// echoes it back on the response, and pushes it onto Serilog's LogContext so every log line emitted
    /// while handling the request carries the same id for end-to-end tracing.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var incoming) && !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
