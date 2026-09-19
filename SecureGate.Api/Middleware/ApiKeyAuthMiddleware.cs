using SecureGate.Application.Interfaces;
using SecureGate.Domain.Enums;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";
    private static readonly TimeSpan ValidTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan NegativeTtl = TimeSpan.FromSeconds(30);

    public ApiKeyAuthMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Validates the key against a short-lived Redis cache before touching SQL. Only a lightweight
    /// <see cref="CachedApiKey"/> is stored, never the EF entity, so no navigation graph or secret such as a
    /// password hash can leak into the cache. Unknown or suspended keys are stored as a negative entry for a
    /// shorter window, so a flood of bogus keys cannot repeatedly hammer SQL (cache penetration). The TTL
    /// bounds how long a suspension can lag; suspend logic should also call
    /// <see cref="ICacheService.RemoveAsync"/> for immediate invalidation.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IApiKeyRepository apiKeyRepository, ICacheService cacheService)
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

        var cacheKey = $"apikey:{keyValue}";
        var entry = await cacheService.GetAsync<ApiKeyCacheEntry>(cacheKey);

        if (entry is null)
        {
            var apiKey = await apiKeyRepository.GetByKeyValueAsync(keyValue!);

            if (apiKey is not null && apiKey.Status == KeyStatus.Active)
            {
                entry = new ApiKeyCacheEntry(CachedApiKey.FromEntity(apiKey));
                await cacheService.SetAsync(cacheKey, entry, ValidTtl);
            }
            else
            {
                entry = new ApiKeyCacheEntry(null);
                await cacheService.SetAsync(cacheKey, entry, NegativeTtl);
            }
        }

        if (entry.Key is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Geçersiz veya askıya alınmış API key.");
            return;
        }

        context.Items["ApiKey"] = entry.Key;

        await _next(context);
    }
}
