using SecureGate.Domain.Entities;

namespace SecureGate.Domain.Interfaces;

public interface IUsageRepository : IGenericRepository<UsageRecord>
{
    Task<List<UsageRecord>> GetRecentByApiKeyIdAsync(Guid apiKeyId, DateTime since);

    Task<List<UsageRecord>> GetRecentAsync(DateTime since);
}