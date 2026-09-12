using SecureGate.Domain.Enums;

namespace SecureGate.Domain.Entities;

public class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PlanType Name { get; set; }
    public int RequestsPerMinute { get; set; }
}