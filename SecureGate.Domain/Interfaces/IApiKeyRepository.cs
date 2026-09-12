using SecureGate.Domain.Entities;

namespace SecureGate.Domain.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id);
    Task<ApiKey?> GetByKeyValueAsync(string keyValue);
    Task AddAsync(ApiKey apiKey);
    Task UpdateAsync(ApiKey apiKey);

}