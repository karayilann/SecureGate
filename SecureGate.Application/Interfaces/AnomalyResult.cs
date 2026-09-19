namespace SecureGate.Application.Interfaces;

public sealed record AnomalyResult(Guid ApiKeyId, int DistinctIpCount, string Reason);
