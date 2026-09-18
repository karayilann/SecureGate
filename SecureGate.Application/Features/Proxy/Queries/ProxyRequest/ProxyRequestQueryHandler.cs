using MediatR;
using SecureGate.Application.Interfaces;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public class ProxyRequestQueryHandler : IRequestHandler<ProxyRequestQuery, BackendResponse>
{
    private readonly IBackendService _backendService;

    public ProxyRequestQueryHandler(IBackendService backendService) => _backendService = backendService;

    public async Task<BackendResponse> Handle(ProxyRequestQuery request, CancellationToken cancellationToken)
        => await _backendService.FetchAsync(request.Resource, cancellationToken);
}
