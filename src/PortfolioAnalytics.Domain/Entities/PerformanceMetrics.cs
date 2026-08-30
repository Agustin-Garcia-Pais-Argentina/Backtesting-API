namespace PortfolioAnalytics.Domain.Entities;

public class PerformanceMetrics
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BacktestRunId { get; private set; }
    public decimal TotalReturn { get; private set; }
    public decimal AnnualizedReturn { get; private set; }
    public decimal MaxDrawdown { get; private set; }
    public decimal SharpeRatio { get; private set; }
    public decimal Volatility { get; private set; }
    public int TradeCount { get; private set; }

    public PerformanceMetrics(Guid backtestRunId, decimal totalReturn, decimal annualizedReturn, decimal maxDrawdown, decimal sharpeRatio, decimal volatility, int tradeCount)
    {
        if (backtestRunId == Guid.Empty)
            throw new ArgumentException("Backtest run identifier is required.", nameof(backtestRunId));

        BacktestRunId = backtestRunId;
        TotalReturn = totalReturn;
        AnnualizedReturn = annualizedReturn;
        MaxDrawdown = maxDrawdown;
        SharpeRatio = sharpeRatio;
        Volatility = volatility;
        TradeCount = tradeCount;
    }
}
