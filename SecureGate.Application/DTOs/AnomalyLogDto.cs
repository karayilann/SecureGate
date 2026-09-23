namespace SecureGate.Application.DTOs;

public class AnomalyLogDto
{
    public Guid Id { get; set; }
    public Guid ApiKeyId { get; set; }
    public string MaskedKeyValue { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int DistinctIpCount { get; set; }
    public DateTime DetectedAt { get; set; }
}
