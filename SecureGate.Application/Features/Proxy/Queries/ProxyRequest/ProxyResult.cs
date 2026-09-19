using SecureGate.Application.Interfaces;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public sealed record ProxyResult(BackendResponse Response, bool FromCache);
