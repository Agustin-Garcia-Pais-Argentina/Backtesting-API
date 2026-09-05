namespace PortfolioAnalytics.Application.Commands;

/// <summary>
/// Request model for the first backtest execution.
/// We keep the scope intentionally small: a single symbol, a date window, and a fixed starting capital.
/// </summary>
public sealed record RunBacktestCommand(
    Guid UserId,
    string Symbol,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal InitialCash = 10000m)
{
    public Guid RunId { get; init; }

    // Keep the previous constructor available for application-level callers that do not
    // originate from an authenticated HTTP request. API requests use the owner-aware form.
    public RunBacktestCommand(
        string symbol,
        DateOnly startDate,
        DateOnly endDate,
        decimal initialCash = 10000m)
        : this(Guid.Empty, symbol, startDate, endDate, initialCash)
    {
    }
}
