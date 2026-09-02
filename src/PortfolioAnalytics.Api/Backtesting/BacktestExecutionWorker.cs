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

            currentRun.Status = "Running";
            _store.Save(currentRun);

            try
            {
                var metrics = await _handler.HandleAsync(workItem.Command, stoppingToken);

                currentRun.Status = "Completed";
                currentRun.CompletedAt = DateTime.UtcNow;
                currentRun.TotalReturn = metrics.TotalReturn;
                currentRun.AnnualizedReturn = metrics.AnnualizedReturn;
                currentRun.MaxDrawdown = metrics.MaxDrawdown;
                currentRun.SharpeRatio = metrics.SharpeRatio;
                currentRun.Volatility = metrics.Volatility;
                currentRun.TradeCount = metrics.TradeCount;
                _store.Save(currentRun);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                currentRun.Status = "Failed";
                currentRun.CompletedAt = DateTime.UtcNow;
                _store.Save(currentRun);
                _logger.LogError(exception, "Backtest {RunId} failed during background execution.", workItem.RunId);
            }
        }
    }
}
