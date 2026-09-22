namespace SecureGate.Application.Anomaly;

public class AnomalyOptions
{
    public int DistinctIpThreshold { get; set; } = 10;
    public int WindowMinutes { get; set; } = 5;
    public int ScanIntervalSeconds { get; set; } = 180;
}
