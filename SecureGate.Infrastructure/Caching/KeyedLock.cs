using System.Collections.Concurrent;
using SecureGate.Application.Interfaces;

namespace SecureGate.Infrastructure.Caching;

public sealed class KeyedLock : IKeyedLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Returns a per-key mutex; callers await it and dispose the result to release. Registered as a
    /// singleton so all requests for the same key share one semaphore. The dictionary keeps one semaphore
    /// per distinct key seen — fine for a bounded key space; a very large key space would want eviction.
    /// </summary>
    public async Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken = default)
    {
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new Releaser(semaphore);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _semaphore.Release();
        }
    }
}
