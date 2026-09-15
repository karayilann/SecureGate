using SecureGate.Domain.Interfaces;

namespace SecureGate.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IApiKeyRepository apiKeyRepository)
    {
        if (!context.Request.Path.StartsWithSegments("/proxy"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var keyValue) ||
            string.IsNullOrWhiteSpace(keyValue))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key eksik.");
            return;
        }

        var apiKey = await apiKeyRepository.GetByKeyValueAsync(keyValue!);

        if (apiKey is null || apiKey.Status == Domain.Enums.KeyStatus.Suspended)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Geçersiz veya askıya alınmış API key.");
            return;
        }

        context.Items["ApiKey"] = apiKey;

        await _next(context);
    }
}