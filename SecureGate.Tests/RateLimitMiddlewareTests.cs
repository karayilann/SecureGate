using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SecureGate.Api.Middleware;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Enums;
using Xunit;

namespace SecureGate.Tests;

public class RateLimitMiddlewareTests
{
    private readonly Mock<IRateLimiter> _rateLimiterMock = new();
    private bool _nextCalled;

    private RateLimitMiddleware CreateMiddleware()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        };
        return new RateLimitMiddleware(next);
    }

    private static DefaultHttpContext CreateProxyContext(CachedApiKey? apiKey)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/proxy";
        context.Response.Body = new MemoryStream();
        if (apiKey is not null)
        {
            context.Items["ApiKey"] = apiKey;
        }
        return context;
    }

    private static CachedApiKey CachedKey(PlanType planName, int requestsPerMinute) =>
        new(Guid.NewGuid(), planName, requestsPerMinute);

    [Fact]
    public async Task NonProxyPath_NextCalled_LimiterNotCalled()
    {
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/keys";

        await middleware.InvokeAsync(context, _rateLimiterMock.Object);

        _nextCalled.Should().BeTrue();
        _rateLimiterMock.Verify(
            r => r.CheckAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task EnterprisePlan_NextCalled_LimiterNotCalled()
    {
        var middleware = CreateMiddleware();
        var context = CreateProxyContext(CachedKey(PlanType.Enterprise, int.MaxValue));

        await middleware.InvokeAsync(context, _rateLimiterMock.Object);

        _nextCalled.Should().BeTrue();
        _rateLimiterMock.Verify(
            r => r.CheckAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task UnderLimit_NextCalled_RateLimitHeadersSet()
    {
        var middleware = CreateMiddleware();
        var context = CreateProxyContext(CachedKey(PlanType.Free, 10));

        _rateLimiterMock
            .Setup(r => r.CheckAsync(It.IsAny<string>(), 10, It.IsAny<TimeSpan>()))
            .ReturnsAsync(new RateLimitResult(true, 10, 7, 0));

        await middleware.InvokeAsync(context, _rateLimiterMock.Object);

        _nextCalled.Should().BeTrue();
        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("10");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("7");
    }

    [Fact]
    public async Task OverLimit_429Returned_RetryAfterSet_NextNotCalled()
    {
        var middleware = CreateMiddleware();
        var context = CreateProxyContext(CachedKey(PlanType.Free, 10));

        _rateLimiterMock
            .Setup(r => r.CheckAsync(It.IsAny<string>(), 10, It.IsAny<TimeSpan>()))
            .ReturnsAsync(new RateLimitResult(false, 10, 0, 23));

        await middleware.InvokeAsync(context, _rateLimiterMock.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        context.Response.Headers["Retry-After"].ToString().Should().Be("23");
        _nextCalled.Should().BeFalse();
    }
}
