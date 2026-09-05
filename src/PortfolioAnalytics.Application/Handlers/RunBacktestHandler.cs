using PortfolioAnalytics.Application.Commands;

using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;
using PortfolioAnalytics.Domain.Services;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>Coordinates market-data retrieval and Domain backtest evaluation.</summary>
public sealed class RunBacktestHandler
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly BacktestCalculator _backtestCalculator;

    public RunBacktestHandler(IMarketDataRepository marketDataRepository, BacktestCalculator backtestCalculator)
    {
        _marketDataRepository = marketDataRepository;
        _backtestCalculator = backtestCalculator;
    }

    public async Task<PerformanceMetrics> HandleAsync(RunBacktestCommand command,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Symbol))
            throw new ArgumentException("Symbol is required.", nameof(command));

        if (command.StartDate > command.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.", nameof(command));

        if (command.InitialCash <= 0)
            throw new ArgumentException("Initial cash must be greater than zero.", nameof(command));

        var series = await _marketDataRepository.GetBySymbolAsync(
            command.Symbol,
            command.StartDate,
            command.EndDate,
            cancellationToken);var data = series.ToList();

        if (data.Count == 0)
            throw new InvalidOperationException("No market data available for the requested symbol and date range.");

        return _backtestCalculator.EvaluateBuyAndHold(command.RunId, data, command.InitialCash);
    }
}
