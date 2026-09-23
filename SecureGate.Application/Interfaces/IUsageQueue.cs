using SecureGate.Domain.Entities;

namespace SecureGate.Application.Interfaces;

public interface IUsageQueue
{
    bool TryEnqueue(UsageRecord record);
}
