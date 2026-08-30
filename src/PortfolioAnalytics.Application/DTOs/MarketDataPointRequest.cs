namespace PortfolioAnalytics.Application.DTOs;

/// <summary>
/// Request payload used to insert or refresh a market data point.
/// </summary>
public sealed class MarketDataPointRequest
{
    public string Symbol { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public string Source { get; set; } = "manual";
}

/// <summary>
/// Response payload for a market data point.
/// </summary>
public sealed class MarketDataPointResponse
{
    public string Symbol { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public string Source { get; set; } = string.Empty;
}
