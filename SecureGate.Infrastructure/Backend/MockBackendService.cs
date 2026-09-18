using SecureGate.Application.Interfaces;

namespace SecureGate.Infrastructure.Backend;

public sealed class MockBackendService : IBackendService
{
    private static readonly TimeSpan SimulatedWork = TimeSpan.FromMilliseconds(800);

    /// <summary>
    /// Simulates a slow upstream call so the caching layer added later has a real latency to remove.
    /// Nothing is actually fetched; the payload size is derived deterministically from the resource key,
    /// so the same request always yields the same response.
    /// </summary>
    public async Task<BackendResponse> FetchAsync(string resource, CancellationToken cancellationToken = default)
    {
        await Task.Delay(SimulatedWork, cancellationToken);

        var payloadSizeBytes = 1_000_000 + DeterministicSeed(resource);

        return new BackendResponse(resource, payloadSizeBytes);
    }

    private static long DeterministicSeed(string resource)
    {
        long seed = 17;
        foreach (var c in resource)
        {
            seed = (seed * 31 + c) % 3_000_000;
        }
        return seed;
    }
}
