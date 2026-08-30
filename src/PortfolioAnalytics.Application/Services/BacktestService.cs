using PortfolioAnalytics.Domain.Entities;

namespace PortfolioAnalytics.Application.Services;

/// <summary>
/// Executes the first deterministic investment strategy used by the MVP.
/// We intentionally keep the logic explicit: buy and hold, one asset, and transparent metrics.
/// </summary>
public sealed class BacktestService
{
    public PerformanceMetrics RunBuyAndHold(IEnumerable<MarketDataPoint> series, decimal initialCash = 10000m)
    {
        if (series is null)
            throw new ArgumentNullException(nameof(series));

        var orderedSeries = series
            .OrderBy(point => point.Date)
            .ToList();

        if (orderedSeries.Count == 0)
            throw new ArgumentException("Market data series cannot be empty.", nameof(series));

        if (initialCash <= 0)
            throw new ArgumentException("Initial cash must be greater than zero.", nameof(initialCash));

        var startPrice = orderedSeries[0].Close;
        var endPrice = orderedSeries[^1].Close;

        if (startPrice <= 0)
            throw new InvalidOperationException("The starting close price must be greater than zero.");

        var totalReturn = (endPrice - startPrice) / startPrice;
        var years = GetYearsBetween(orderedSeries[0].Date, orderedSeries[^1].Date);
        var annualizedReturn = (decimal)Math.Pow((double)(endPrice / startPrice), 1d / years) - 1m;

        var drawdown = CalculateMaxDrawdown(orderedSeries);
        var returns = CalculateReturns(orderedSeries);
        var volatility = CalculateVolatility(returns);
        var sharpe = volatility == 0 ? 0m : annualizedReturn / volatility;
        var tradeCount = returns.Count > 0 ? 1 : 0;

        return new PerformanceMetrics(
            Guid.NewGuid(),
            totalReturn,
            annualizedReturn,
            drawdown,
            sharpe,
            volatility,
            tradeCount);
    }

    private static double GetYearsBetween(DateOnly from, DateOnly to)
    {
        var days = Math.Max(1d, (to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).TotalDays);
        return days / 365d;
    }

    private static decimal CalculateMaxDrawdown(IReadOnlyList<MarketDataPoint> series)
    {
        var peak = series[0].Close;
        var maxDrawdown = 0m;

        foreach (var point in series)
        {
            if (point.Close > peak)
                peak = point.Close;

            if (peak == 0)
                continue;

            var drawdown = (peak - point.Close) / peak;
            if (drawdown > maxDrawdown)
                maxDrawdown = drawdown;
        }

        return maxDrawdown;
    }

    private static List<decimal> CalculateReturns(IReadOnlyList<MarketDataPoint> series)
    {
        var returns = new List<decimal>();

        for (var i = 1; i < series.Count; i++)
        {
            var previousClose = series[i - 1].Close;
            if (previousClose == 0)
                continue;

            returns.Add((series[i].Close - previousClose) / previousClose);
        }

        return returns;
    }

    private static decimal CalculateVolatility(IReadOnlyList<decimal> returns)
    {
        if (returns.Count == 0)
            return 0m;

        var average = returns.Average();
        var variance = returns
            .Select(value => (decimal)Math.Pow((double)(value - average), 2))
            .Average();

        var stdDev = (decimal)Math.Sqrt((double)variance);
        return stdDev * (decimal)Math.Sqrt(252d);
    }
}
