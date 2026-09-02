using PortfolioAnalytics.Application.Handlers;
using PortfolioAnalytics.Application.Services;

namespace PortfolioAnalytics.Api.Backtesting;

/// <summary>
/// Processes queued backtests outside the HTTP request lifecycle.
/// </summary>
public sealed class BacktestExecutionWorker : BackgroundService
{
    private readonly BacktestExecutionQueue _queue;
    private readonly RunBacktestHandler _handler;
    private readonly BacktestExecutionStore _store;
    private readonly ILogger<BacktestExecutionWorker> _logger;

    public BacktestExecutionWorker(
        BacktestExecutionQueue queue,
        RunBacktestHandler handler,
        BacktestExecutionStore store,
        ILogger<BacktestExecutionWorker> logger)
    {
        _queue = queue;
        _handler = handler;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _queue.ReadAllAsync(stoppingToken))
        {
            var currentRun = _store.GetById(workItem.RunId);
            if (currentRun is null)
            {
                _logger.LogWarning("Queued backtest {RunId} was not found.", workItem.RunId);
                continue;
            }

            _store.Update(workItem.RunId, run => run with { Status = "Running" });

            try
            {
                var metrics = await _handler.HandleAsync(workItem.Command, stoppingToken);

                _store.Update(workItem.RunId, run => run with
                {
                    Status = "Completed",
                    CompletedAt = DateTime.UtcNow,
                    TotalReturn = metrics.TotalReturn,
                    AnnualizedReturn = metrics.AnnualizedReturn,
                    MaxDrawdown = metrics.MaxDrawdown,
                    SharpeRatio = metrics.SharpeRatio,
                    Volatility = metrics.Volatility,
                    TradeCount = metrics.TradeCount
                });
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _store.Update(workItem.RunId, run => run with
                {
                    Status = "Failed",
                    CompletedAt = DateTime.UtcNow
                });
                _logger.LogError(exception, "Backtest {RunId} failed during background execution.", workItem.RunId);
            }
        }
    }
}
