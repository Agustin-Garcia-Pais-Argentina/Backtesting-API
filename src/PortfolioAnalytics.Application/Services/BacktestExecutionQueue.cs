using System.Threading.Channels;
using PortfolioAnalytics.Application.Commands;

namespace PortfolioAnalytics.Application.Services;

/// <summary>
/// In-memory queue used to decouple backtest requests from their execution.
/// </summary>
public sealed class BacktestExecutionQueue
{
    private readonly Channel<BacktestWorkItem> _queue = Channel.CreateUnbounded<BacktestWorkItem>();

    public ValueTask EnqueueAsync(BacktestWorkItem workItem, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(workItem, cancellationToken);
    }

    public IAsyncEnumerable<BacktestWorkItem> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}

public sealed record BacktestWorkItem(Guid RunId, RunBacktestCommand Command);
