namespace SecureGate.Application.DTOs;

public class UsageStatsDto
{
    public List<UsageByKeyDto> PerKey { get; set; } = new();
    public List<UsageTimelinePointDto> Timeline { get; set; } = new();
}

public class UsageByKeyDto
{
    public Guid ApiKeyId { get; set; }
    public string MaskedKeyValue { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UsageTimelinePointDto
{
    public DateTime Bucket { get; set; }
    public int Count { get; set; }
}
