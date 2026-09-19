using SecureGate.Domain.Entities;

namespace SecureGate.Application.Interfaces;

public interface IAnomalyDetector
{
    IReadOnlyCollection<AnomalyResult> Detect(IEnumerable<UsageRecord> records);
}
