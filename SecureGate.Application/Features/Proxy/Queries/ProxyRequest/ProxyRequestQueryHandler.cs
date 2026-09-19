using MediatR;
using SecureGate.Application.Interfaces;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public class ProxyRequestQueryHandler : IRequestHandler<ProxyRequestQuery, ProxyResult>
{
    private readonly IBackendService _backendService;
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public ProxyRequestQueryHandler(IBackendService backendService, ICacheService cacheService)
    {
        _backendService = backendService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Cache-aside: a hit returns the stored response (carrying its original GeneratedAtUtc, which is how
    /// a caller can tell the answer came from cache); a miss calls the slow backend once, stores the
    /// result under a TTL, and returns it.
    /// </summary>
    public async Task<ProxyResult> Handle(ProxyRequestQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"proxy:{request.Resource}";

        var cached = await _cacheService.GetAsync<BackendResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return new ProxyResult(cached, true);
        }

        var response = await _backendService.FetchAsync(request.Resource, cancellationToken);
        await _cacheService.SetAsync(cacheKey, response, CacheTtl, cancellationToken);

        return new ProxyResult(response, false);
    }
}
