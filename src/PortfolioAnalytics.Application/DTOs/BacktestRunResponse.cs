namespace PortfolioAnalytics.Application.DTOs;

/// <summary>
/// API response model for a completed backtest run.
/// </summary>
public sealed class BacktestRunResponse
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal InitialCapital { get; set; }
    public string StrategyType { get; set; } = "BuyAndHold";
    public string Status { get; set; } = "Completed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal Volatility { get; set; }
    public int TradeCount { get; set; }
}
