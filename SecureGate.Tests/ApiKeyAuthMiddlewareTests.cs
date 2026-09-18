using Microsoft.AspNetCore.Http;
using Moq;
using SecureGate.Api.Middleware;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;
using SecureGate.Domain.Interfaces;
using Xunit;

namespace SecureGate.Tests;

public class ApiKeyAuthMiddlewareTests
{
    private readonly Mock<IApiKeyRepository> _repositoryMock = new();
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

        await middleware.InvokeAsync(context, _repositoryMock.Object);

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

        await middleware.InvokeAsync(context, _repositoryMock.Object);

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

        await middleware.InvokeAsync(context, _repositoryMock.Object);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task GecerliVeAktifKey_NextCagrilmali_ContextItemsDolmali()
    {
        var middleware = CreateMiddleware();
        var context = CreateContextForProxyPath();
        context.Request.Headers["X-Api-Key"] = "gecerli-key";

        var activeKey = new ApiKey { KeyValue = "gecerli-key", Status = KeyStatus.Active };

        _repositoryMock
            .Setup(r => r.GetByKeyValueAsync("gecerli-key"))
            .ReturnsAsync(activeKey);

        await middleware.InvokeAsync(context, _repositoryMock.Object);

        Assert.True(_nextCalled);
        Assert.Equal(activeKey, context.Items["ApiKey"]);
    }

    [Fact]
    public async Task ProxyDisiPath_MiddlewareDevreDisiKalmali()
    {
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/keys"; // /proxy altında değil

        await middleware.InvokeAsync(context, _repositoryMock.Object);

        // Header hiç kontrol edilmeden doğrudan geçmeli
        Assert.True(_nextCalled);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }
}