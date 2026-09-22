using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;

namespace SecureGate.Application.Anomaly;

public sealed class AnomalyDetector : IAnomalyDetector
{
    /// <summary>
    /// Flags a key when it was used from at least <paramref name="distinctIpThreshold"/> different IP
    /// addresses within the records it is given (the caller narrows those to the detection window). Pure and
    /// side-effect free so it can be unit tested with fabricated records.
    /// </summary>
    public IReadOnlyCollection<AnomalyResult> Detect(IEnumerable<UsageRecord> records, int distinctIpThreshold)
    {
        return records
            .GroupBy(record => record.ApiKeyId)
            .Select(group => new
            {
                ApiKeyId = group.Key,
                DistinctIpCount = group.Select(record => record.IpAddress).Distinct().Count()
            })
            .Where(candidate => candidate.DistinctIpCount >= distinctIpThreshold)
            .Select(candidate => new AnomalyResult(
                candidate.ApiKeyId,
                candidate.DistinctIpCount,
                $"{candidate.DistinctIpCount} distinct IP addresses in the detection window"))
            .ToList();
    }
}
