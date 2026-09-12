
using SecureGate.Domain.Entities;

namespace SecureGate.Domain.Interfaces
{
    public interface IUsageRepository
    {
        Task AddAsync(UsageRecord usage);
        Task<List<UsageRecord>> GetRecentByApiKeyIdAsync(Guid apiKeyId, DateTime since);
    }
}
