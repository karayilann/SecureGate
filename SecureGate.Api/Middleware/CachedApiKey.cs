using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;

namespace SecureGate.Api.Middleware;

public sealed record CachedApiKey(Guid Id, PlanType PlanName, int RequestsPerMinute)
{
    public static CachedApiKey FromEntity(ApiKey apiKey) =>
        new(apiKey.Id, apiKey.Plan!.Name, apiKey.Plan.RequestsPerMinute);
}
