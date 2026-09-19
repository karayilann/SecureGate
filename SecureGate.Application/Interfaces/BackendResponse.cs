namespace SecureGate.Application.Interfaces;

public sealed record BackendResponse(string Resource, long PayloadSizeBytes, DateTime GeneratedAtUtc);
