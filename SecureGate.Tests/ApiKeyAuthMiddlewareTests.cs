using Microsoft.AspNetCore.Http;
using Moq;
using SecureGate.Api.Middleware;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;
using SecureGate.Domain.Interfaces;
using Xunit;

namespace SecureGate.Tests;

public class ApiKeyAuthMiddlewareTests
{
    private readonly Mock<IApiKeyRepository> _repositoryMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private bool _nextCalled;

    private ApiKeyAuthMiddleware CreateMiddleware()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        };
        return new ApiKeyAuthMiddleware(next);
    }

    private static DefaultHttpContext CreateContextForProxyPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/proxy";
        return context;
    }

    [Fact]
    public async Task HeaderYok_401Donmeli_NextCagrilmamali()
    {
        var middleware = CreateMiddleware();
        var context = CreateContextForProxyPath();

        await middleware.InvokeAsync(context, _repositoryMock.Object, _cacheServiceMock.Object);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task GecersizKey_401Donmeli_NextCagrilmamali()
    {
        var middleware = CreateMiddleware();
        var context = CreateContextForProxyPath();
        context.Request.Headers["X-Api-Key"] = "olmayan-key";

        _repositoryMock
            .Setup(r => r.GetByKeyValueAsync("olmayan-key"))
            .ReturnsAsync((ApiKey?)null);

        await middleware.InvokeAsync(context, _repositoryMock.Object, _cacheServiceMock.Object);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task SuspendedKey_401Donmeli_NextCagrilmamali()
    {
        var middleware = CreateMiddleware();
        var context = CreateContextForProxyPath();
        context.Request.Headers["X-Api-Key"] = "suspended-key";

        var suspendedKey = new ApiKey { KeyValue = "suspended-key", Status = KeyStatus.Suspended };

        _repositoryMock
            .Setup(r => r.GetByKeyValueAsync("suspended-key"))
            .ReturnsAsync(suspendedKey);

        await middleware.InvokeAsync(context, _repositoryMock.Object, _cacheServiceMock.Object);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task GecerliVeAktifKey_NextCagrilmali_ContextItemsDolmali()
    {
        var middleware = CreateMiddleware();
        var context = CreateContextForProxyPath();
        context.Request.Headers["X-Api-Key"] = "gecerli-key";

        var activeKey = new ApiKey
        {
            KeyValue = "gecerli-key",
            Status = KeyStatus.Active,
            Plan = new Plan { Name = PlanType.Free, RequestsPerMinute = 10 }
        };

        _repositoryMock
            .Setup(r => r.GetByKeyValueAsync("gecerli-key"))
            .ReturnsAsync(activeKey);

        await middleware.InvokeAsync(context, _repositoryMock.Object, _cacheServiceMock.Object);

        Assert.True(_nextCalled);
        var cached = Assert.IsType<CachedApiKey>(context.Items["ApiKey"]);
        Assert.Equal(activeKey.Id, cached.Id);
        Assert.Equal(PlanType.Free, cached.PlanName);
    }

    [Fact]
    public async Task ProxyDisiPath_MiddlewareDevreDisiKalmali()
    {
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/keys"; // /proxy altında değil

        await middleware.InvokeAsync(context, _repositoryMock.Object, _cacheServiceMock.Object);

        // Header hiç kontrol edilmeden doğrudan geçmeli
        Assert.True(_nextCalled);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task CachedKey_ServedFromCache_RepositoryNotCalled()
    {
        var middleware = CreateMiddleware();
        var context = CreateContextForProxyPath();
        context.Request.Headers["X-Api-Key"] = "cached-key";

        var cachedKey = new CachedApiKey(Guid.NewGuid(), PlanType.Free, 10);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ApiKeyCacheEntry>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyCacheEntry(cachedKey));

        await middleware.InvokeAsync(context, _repositoryMock.Object, _cacheServiceMock.Object);

        Assert.True(_nextCalled);
        Assert.Equal(cachedKey, context.Items["ApiKey"]);
        _repositoryMock.Verify(r => r.GetByKeyValueAsync(It.IsAny<string>()), Times.Never);
    }
}