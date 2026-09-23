using MediatR;
using SecureGate.Application.Interfaces;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public class ProxyRequestQueryHandler : IRequestHandler<ProxyRequestQuery, ProxyResult>
{
    private readonly IBackendService _backendService;
    private readonly ICacheService _cacheService;
    private readonly IKeyedLock _keyedLock;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public ProxyRequestQueryHandler(
        IBackendService backendService,
        ICacheService cacheService,
        IKeyedLock keyedLock)
    {
        _backendService = backendService;
        _cacheService = cacheService;
        _keyedLock = keyedLock;
    }

    /// <summary>
    /// Cache-aside with stampede protection: a hit returns immediately; on a miss only one request per
    /// resource passes the keyed lock and calls the slow backend, while the rest wait and then read the
    /// value it stored. The second cache check inside the lock is the double-check that lets those waiters
    /// return the freshly cached response instead of all hitting the backend at once.
    /// </summary>
    public async Task<ProxyResult> Handle(ProxyRequestQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"proxy:{request.Resource}";

        var cached = await _cacheService.GetAsync<BackendResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return new ProxyResult(cached, true);
        }

        using (await _keyedLock.AcquireAsync(cacheKey, cancellationToken))
        {
            cached = await _cacheService.GetAsync<BackendResponse>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return new ProxyResult(cached, true);
            }

            var response = await _backendService.FetchAsync(request.Resource, cancellationToken);
            await _cacheService.SetAsync(cacheKey, response, CacheTtl, cancellationToken);

            return new ProxyResult(response, false);
        }
    }
}
