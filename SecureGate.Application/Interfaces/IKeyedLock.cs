namespace SecureGate.Application.Interfaces;

public interface IKeyedLock
{
    Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken = default);
}
