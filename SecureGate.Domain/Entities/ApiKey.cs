using SecureGate.Domain.Enums;

namespace SecureGate.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string KeyValue { get; set; } = string.Empty;

    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }

    public KeyStatus Status { get; set; } = KeyStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}