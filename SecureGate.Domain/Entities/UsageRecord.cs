namespace SecureGate.Domain.Entities;

public class UsageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApiKeyId { get; set; }
    public ApiKey? ApiKey { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}