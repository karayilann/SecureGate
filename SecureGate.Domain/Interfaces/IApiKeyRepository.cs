using SecureGate.Domain.Entities;

namespace SecureGate.Domain.Interfaces;

public interface IApiKeyRepository : IGenericRepository<ApiKey>
{
    Task<ApiKey?> GetByKeyValueAsync(string keyValue);
}