namespace PortfolioAnalytics.Domain.Entities;

/// <summary>
/// Represents a single OHLCV bar for a symbol and source.
/// This entity is the foundation for all historical market analysis and later backtesting workflows.
/// </summary>
public class MarketDataPoint
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Symbol { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public decimal Open { get; private set; }
    public decimal High { get; private set; }
    public decimal Low { get; private set; }
    public decimal Close { get; private set; }
    public decimal Volume { get; private set; }
    public string Source { get; private set; } = string.Empty;

    /// <summary>
    /// Validates the minimum invariants required for a meaningful market data record.
    /// </summary>
    public MarketDataPoint(string symbol, DateOnly date, decimal open, decimal high, decimal low, decimal close, decimal volume, string source)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol is required.", nameof(symbol));

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source is required.", nameof(source));

        if (high < low)
            throw new ArgumentException("High price cannot be lower than low price.", nameof(high));

        Symbol = symbol.Trim().ToUpperInvariant();
        Date = date;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        Source = source.Trim();
    }
}
