using MediatR;
using SecureGate.Application.Interfaces;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public record ProxyRequestQuery(string Resource) : IRequest<BackendResponse>;
