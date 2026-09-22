using FluentAssertions;
using SecureGate.Application.Anomaly;
using SecureGate.Domain.Entities;
using Xunit;

namespace SecureGate.Tests;

public class AnomalyDetectorTests
{
    private const int DistinctIpThreshold = 10;
    private readonly AnomalyDetector _detector = new();

    private static UsageRecord Usage(Guid apiKeyId, string ip) => new()
    {
        ApiKeyId = apiKeyId,
        IpAddress = ip
    };

    private static IEnumerable<UsageRecord> UsageFromDistinctIps(Guid apiKeyId, int distinctIpCount)
    {
        for (var i = 0; i < distinctIpCount; i++)
        {
            yield return Usage(apiKeyId, $"10.0.0.{i}");
        }
    }

    [Fact]
    public void BelowThreshold_NotFlagged()
    {
        var apiKeyId = Guid.NewGuid();

        var result = _detector.Detect(UsageFromDistinctIps(apiKeyId, 9), DistinctIpThreshold);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AtThreshold_Flagged()
    {
        var apiKeyId = Guid.NewGuid();

        var result = _detector.Detect(UsageFromDistinctIps(apiKeyId, 10), DistinctIpThreshold);

        result.Should().ContainSingle();
        result.Single().ApiKeyId.Should().Be(apiKeyId);
        result.Single().DistinctIpCount.Should().Be(10);
    }

    [Fact]
    public void RepeatedSameIps_CountsDistinctOnly_NotFlagged()
    {
        var apiKeyId = Guid.NewGuid();
        var records = Enumerable.Range(0, 30).Select(i => Usage(apiKeyId, $"10.0.0.{i % 5}"));

        var result = _detector.Detect(records, DistinctIpThreshold);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MultipleKeys_OnlySuspiciousFlagged()
    {
        var suspiciousKey = Guid.NewGuid();
        var normalKey = Guid.NewGuid();
        var records = UsageFromDistinctIps(suspiciousKey, 12)
            .Concat(UsageFromDistinctIps(normalKey, 3))
            .ToList();

        var result = _detector.Detect(records, DistinctIpThreshold);

        result.Should().ContainSingle();
        result.Single().ApiKeyId.Should().Be(suspiciousKey);
        result.Single().DistinctIpCount.Should().Be(12);
    }
}
