using SecureGate.Domain.Entities;

namespace SecureGate.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
