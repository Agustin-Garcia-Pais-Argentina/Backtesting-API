namespace PortfolioAnalytics.Application.DTOs;

/// <summary>
/// API response model for a completed backtest run.
/// </summary>
public sealed record BacktestRunResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal InitialCapital { get; init; }
    public string StrategyType { get; init; } = "BuyAndHold";
    public string Status { get; init; } = "Queued";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; init; }
    public decimal TotalReturn { get; init; }
    public decimal AnnualizedReturn { get; init; }
    public decimal MaxDrawdown { get; init; }
    public decimal SharpeRatio { get; init; }
    public decimal Volatility { get; init; }
    public int TradeCount { get; init; }
}
