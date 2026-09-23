using FluentAssertions;
using Moq;
using SecureGate.Application.Features.Proxy.Queries.ProxyRequest;
using SecureGate.Application.Interfaces;
using SecureGate.Infrastructure.Caching;
using Xunit;

namespace SecureGate.Tests;

public class ProxyRequestQueryHandlerTests
{
    private readonly Mock<IBackendService> _backendMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    private ProxyRequestQueryHandler CreateHandler() => new(_backendMock.Object, _cacheMock.Object, new KeyedLock());

    [Fact]
    public async Task CacheMiss_CallsBackendOnce_StoresInCache_FromCacheFalse()
    {
        var backendResponse = new BackendResponse("abc", 1234, DateTime.UtcNow);

        _cacheMock
            .Setup(c => c.GetAsync<BackendResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BackendResponse?)null);
        _backendMock
            .Setup(b => b.FetchAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(backendResponse);

        var result = await CreateHandler().Handle(new ProxyRequestQuery("abc"), TestContext.Current.CancellationToken);

        result.FromCache.Should().BeFalse();
        result.Response.Should().Be(backendResponse);
        _backendMock.Verify(b => b.FetchAsync("abc", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.SetAsync(It.IsAny<string>(), backendResponse, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CacheHit_DoesNotCallBackend_FromCacheTrue()
    {
        var cachedResponse = new BackendResponse("abc", 1234, DateTime.UtcNow);

        _cacheMock
            .Setup(c => c.GetAsync<BackendResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        var result = await CreateHandler().Handle(new ProxyRequestQuery("abc"), TestContext.Current.CancellationToken);

        result.FromCache.Should().BeTrue();
        result.Response.Should().Be(cachedResponse);
        _backendMock.Verify(
            b => b.FetchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
