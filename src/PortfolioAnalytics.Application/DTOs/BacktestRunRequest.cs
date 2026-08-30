namespace PortfolioAnalytics.Application.DTOs;

/// <summary>
/// Input payload for running a simple backtest against a symbol and time window.
/// </summary>
public sealed class BacktestRunRequest
{
    public string Symbol { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal InitialCapital { get; set; } = 10000m;
}
