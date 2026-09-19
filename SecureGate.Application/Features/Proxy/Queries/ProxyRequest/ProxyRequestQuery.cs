using MediatR;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public record ProxyRequestQuery(string Resource) : IRequest<ProxyResult>;
