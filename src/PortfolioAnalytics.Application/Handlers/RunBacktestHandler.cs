using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.Services;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>
/// Executes the first simple backtest flow for a symbol and date range.
/// The handler keeps the API thin and delegates the actual analytical computations to the service layer.
/// </summary>
public sealed class RunBacktestHandler
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly BacktestService _backtestService;

    public RunBacktestHandler(IMarketDataRepository marketDataRepository, BacktestService backtestService)
    {
        _marketDataRepository = marketDataRepository;
        _backtestService = backtestService;
    }

    public async Task<PerformanceMetrics> HandleAsync(RunBacktestCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Symbol))
            throw new ArgumentException("Symbol is required.", nameof(command));

        if (command.StartDate > command.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.", nameof(command));

        if (command.InitialCash <= 0)
            throw new ArgumentException("Initial cash must be greater than zero.", nameof(command));

        var series = await _marketDataRepository.GetBySymbolAsync(command.Symbol, command.StartDate, command.EndDate, cancellationToken);
        var data = series.ToList();

        if (data.Count == 0)
            throw new InvalidOperationException("No market data available for the requested symbol and date range.");

        return _backtestService.RunBuyAndHold(data, command.InitialCash);
    }
}
