using System.Threading.Channels;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Entities;

namespace SecureGate.Infrastructure.BackgroundJobs;

public sealed class UsageQueue : IUsageQueue
{
    private readonly Channel<UsageRecord> _channel = Channel.CreateBounded<UsageRecord>(
        new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true
        });

    /// <summary>
    /// Best-effort: drops the record if the queue is full rather than blocking the request thread, since
    /// usage logging must never slow down or fail a proxied request.
    /// </summary>
    public bool TryEnqueue(UsageRecord record) => _channel.Writer.TryWrite(record);

    public ChannelReader<UsageRecord> Reader => _channel.Reader;
}
