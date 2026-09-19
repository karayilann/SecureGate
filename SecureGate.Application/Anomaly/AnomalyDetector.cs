using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;

namespace SecureGate.Application.Anomaly;

public sealed class AnomalyDetector : IAnomalyDetector
{
    private const int DistinctIpThreshold = 10;

    /// <summary>
    /// Flags a key when it was used from at least <c>DistinctIpThreshold</c> different IP addresses within
    /// the records it is given (the caller narrows those to the detection window). Pure and side-effect free
    /// so it can be unit tested with fabricated records.
    /// </summary>
    public IReadOnlyCollection<AnomalyResult> Detect(IEnumerable<UsageRecord> records)
    {
        return records
            .GroupBy(record => record.ApiKeyId)
            .Select(group => new
            {
                ApiKeyId = group.Key,
                DistinctIpCount = group.Select(record => record.IpAddress).Distinct().Count()
            })
            .Where(candidate => candidate.DistinctIpCount >= DistinctIpThreshold)
            .Select(candidate => new AnomalyResult(
                candidate.ApiKeyId,
                candidate.DistinctIpCount,
                $"{candidate.DistinctIpCount} distinct IP addresses in the detection window"))
            .ToList();
    }
}
