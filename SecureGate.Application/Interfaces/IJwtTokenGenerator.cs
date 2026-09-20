using SecureGate.Domain.Entities;

namespace SecureGate.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
