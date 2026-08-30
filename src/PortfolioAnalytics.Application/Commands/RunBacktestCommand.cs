namespace PortfolioAnalytics.Application.Commands;

/// <summary>
/// Request model for the first backtest execution.
/// We keep the scope intentionally small: a single symbol, a date window, and a fixed starting capital.
/// </summary>
public sealed record RunBacktestCommand(
    string Symbol,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal InitialCash = 10000m);
