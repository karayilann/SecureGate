using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using SecureGate.Application.Interfaces;

namespace SecureGate.Infrastructure.Backend;

public sealed class ResilientBackendService : IBackendService
{
    private readonly IBackendService _inner;
    private readonly ResiliencePipeline _pipeline;

    /// <summary>
    /// Wraps the real backend call in retry, circuit breaker, and per-attempt timeout so a slow or failing
    /// upstream is retried a couple of times, then short-circuited (fail fast) instead of hanging every
    /// request. The pipeline is built once because the circuit breaker must share state across requests.
    /// </summary>
    public ResilientBackendService(IBackendService inner)
    {
        _inner = inner;
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15)
            })
            .AddTimeout(TimeSpan.FromSeconds(2))
            .Build();
    }

    public async Task<BackendResponse> FetchAsync(string resource, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(async token => await _inner.FetchAsync(resource, token), cancellationToken);
    }
}
