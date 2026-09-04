using System.Threading.Channels;
using PortfolioAnalytics.Application.Commands;

namespace PortfolioAnalytics.Application.Services;

/// <summary>
/// In-memory queue used to decouple backtest requests from their execution.
/// </summary>
public sealed class BacktestExecutionQueue
{
    public const int DefaultCapacity = 100;

    private readonly Channel<BacktestWorkItem> _queue;

    public BacktestExecutionQueue(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be greater than zero.");
        }

        Capacity = capacity;
        _queue = Channel.CreateBounded<BacktestWorkItem>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public int Capacity { get; }

    public ValueTask EnqueueAsync(BacktestWorkItem workItem, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_queue.Writer.TryWrite(workItem))
        {
            return ValueTask.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromException(new BacktestQueueFullException(Capacity));
    }

    public IAsyncEnumerable<BacktestWorkItem> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}

public sealed class BacktestQueueFullException : InvalidOperationException
{
    public BacktestQueueFullException(int capacity)
        : base($"The backtest queue is full (capacity: {capacity}).")
    {
        Capacity = capacity;
    }

    public int Capacity { get; }
}

public sealed record BacktestWorkItem(Guid RunId, Guid UserId, RunBacktestCommand Command)
{
    // Compatibility constructor for non-HTTP callers; authenticated API requests use
    // the explicit owner-aware constructor below.
    public BacktestWorkItem(Guid runId, RunBacktestCommand command)
        : this(runId, command.UserId, command)
    {
    }
}
