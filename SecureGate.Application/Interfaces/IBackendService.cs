namespace SecureGate.Application.Interfaces;

public interface IBackendService
{
    Task<BackendResponse> FetchAsync(string resource, CancellationToken cancellationToken = default);
}
