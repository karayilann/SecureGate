using FluentAssertions;
using SecureGate.Infrastructure.RateLimiting;
using StackExchange.Redis;
using Xunit;

namespace SecureGate.Tests;

public sealed class RedisFixture : IDisposable
{
    public IConnectionMultiplexer Connection { get; }

    public RedisFixture() => Connection = ConnectionMultiplexer.Connect("localhost:6379");

    public void Dispose() => Connection.Dispose();
}

public class RedisRateLimiterTests : IClassFixture<RedisFixture>
{
    private readonly RedisRateLimiter _rateLimiter;

    public RedisRateLimiterTests(RedisFixture fixture) => _rateLimiter = new RedisRateLimiter(fixture.Connection);

    private static string NewClientId() => Guid.NewGuid().ToString();

    [Fact]
    public async Task RequestsUnderLimit_ShouldBeAllowed()
    {
        var clientId = NewClientId();
        const int limit = 3;
        var window = TimeSpan.FromMinutes(1);

        var first = await _rateLimiter.CheckAsync(clientId, limit, window);
        var second = await _rateLimiter.CheckAsync(clientId, limit, window);
        var third = await _rateLimiter.CheckAsync(clientId, limit, window);

        first.IsAllowed.Should().BeTrue();
        second.IsAllowed.Should().BeTrue();
        third.IsAllowed.Should().BeTrue();

        first.Remaining.Should().Be(2);
        second.Remaining.Should().Be(1);
        third.Remaining.Should().Be(0);
    }

    [Fact]
    public async Task WhenLimitExceeded_ShouldBlockAndReturnRetryAfter()
    {
        var clientId = NewClientId();
        const int limit = 3;
        var window = TimeSpan.FromMinutes(1);

        await _rateLimiter.CheckAsync(clientId, limit, window);
        await _rateLimiter.CheckAsync(clientId, limit, window);
        await _rateLimiter.CheckAsync(clientId, limit, window);
        var blocked = await _rateLimiter.CheckAsync(clientId, limit, window);

        blocked.IsAllowed.Should().BeFalse();
        blocked.Remaining.Should().Be(0);
        blocked.RetryAfterSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task WhenWindowSlides_ShouldAllowAgain()
    {
        var clientId = NewClientId();
        const int limit = 1;
        var window = TimeSpan.FromSeconds(2);

        var first = await _rateLimiter.CheckAsync(clientId, limit, window);
        var second = await _rateLimiter.CheckAsync(clientId, limit, window);

        await Task.Delay(TimeSpan.FromSeconds(2.5));

        var third = await _rateLimiter.CheckAsync(clientId, limit, window);

        first.IsAllowed.Should().BeTrue();
        second.IsAllowed.Should().BeFalse();
        third.IsAllowed.Should().BeTrue();
    }
}
