using PortfolioAnalytics.Domain.Entities;
namespace PortfolioAnalytics.Domain.Services;
/// <summary>Evaluates deterministic buy-and-hold backtests using historical market data.</summary>
public sealed class BacktestCalculator
{
    private const decimal TradingDaysPerYear = 252m;
    private const decimal CalendarDaysPerYear = 365m;
    public PerformanceMetrics EvaluateBuyAndHold(Guid backtestRunId, IEnumerable<MarketDataPoint> series, decimal initialCash = 10_000m)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (backtestRunId == Guid.Empty) throw new ArgumentException("Backtest run identifier is required.", nameof(backtestRunId));
        if (initialCash <= 0m) throw new ArgumentException("Initial cash must be greater than zero.", nameof(initialCash));
        var orderedSeries = series.OrderBy(point => point.Date).ToList();
        if (orderedSeries.Count == 0) throw new ArgumentException("Market data series cannot be empty.", nameof(series));
        var startPrice = orderedSeries[0].Close;
        var endPrice = orderedSeries[^1].Close;
        if (startPrice <= 0m) throw new InvalidOperationException("The starting close price must be greater than zero.");
        var totalReturn = (endPrice - startPrice) / startPrice;
        var days = Math.Max(1d, (orderedSeries[^1].Date.ToDateTime(TimeOnly.MinValue) - orderedSeries[0].Date.ToDateTime(TimeOnly.MinValue)).TotalDays);
        var years = (decimal)(days / (double)CalendarDaysPerYear);
        var annualizedReturn = (decimal)Math.Pow((double)(endPrice / startPrice), (double)(1m / years)) - 1m;
        var maxDrawdown = CalculateMaxDrawdown(orderedSeries);
        var returns = CalculateReturns(orderedSeries);
        var volatility = CalculateVolatility(returns);
        var sharpeRatio = volatility == 0m ? 0m : annualizedReturn / volatility;
        return new PerformanceMetrics(backtestRunId, totalReturn, annualizedReturn, maxDrawdown, sharpeRatio, volatility, returns.Count > 0 ? 1 : 0);
    }
    private static decimal CalculateMaxDrawdown(IReadOnlyList<MarketDataPoint> series)
    {
        var peak = series[0].Close;
        var maxDrawdown = 0m;
        foreach (var point in series)
        {
            if (point.Close > peak) peak = point.Close;
            if (peak == 0m) continue;
            var drawdown = (peak - point.Close) / peak;
            if (drawdown > maxDrawdown) maxDrawdown = drawdown;
        }
        return maxDrawdown;
    }
    private static List<decimal> CalculateReturns(IReadOnlyList<MarketDataPoint> series)
    {
        var returns = new List<decimal>();
        for (var index = 1; index < series.Count; index++)
        {
            var previousClose = series[index - 1].Close;
            if (previousClose != 0m) returns.Add((series[index].Close - previousClose) / previousClose);
        }
        return returns;
    }
    private static decimal CalculateVolatility(IReadOnlyList<decimal> returns)
    {
        if (returns.Count == 0) return 0m;
        var average = returns.Average();
        var variance = returns.Select(value => (value - average) * (value - average)).Average();
        return (decimal)Math.Sqrt((double)variance) * (decimal)Math.Sqrt((double)TradingDaysPerYear);
    }
}