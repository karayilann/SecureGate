using SecureGate.Domain.Entities;

namespace SecureGate.Domain.Interfaces;

public interface IAnomalyLogRepository : IGenericRepository<AnomalyLog>
{
    Task<List<AnomalyLog>> GetRecentAsync(int take);
}
