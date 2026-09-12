using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;

namespace SecureGate.Domain.Interfaces
{
    public interface IPlanRepository
    {
        Task<Plan?> GetByTypeAsync(PlanType planType);
    }
}
