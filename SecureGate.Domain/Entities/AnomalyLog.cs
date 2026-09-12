namespace SecureGate.Domain.Entities;

public class AnomalyLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApiKeyId { get; set; }
    public ApiKey? ApiKey { get; set; }

    public string Reason { get; set; } = string.Empty;
    public int DistinctIpCount { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}