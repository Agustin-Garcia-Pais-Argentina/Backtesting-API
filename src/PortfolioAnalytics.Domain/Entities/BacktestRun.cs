using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Domain.Entities;

public class BacktestRun
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Guid StrategyId { get; private set; }
    public BacktestStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; private set; }
    public string? ResultSummaryJson { get; private set; }

    public BacktestRun(Guid userId, Guid portfolioId, Guid strategyId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (portfolioId == Guid.Empty)
            throw new ArgumentException("Portfolio identifier is required.", nameof(portfolioId));

        if (strategyId == Guid.Empty)
            throw new ArgumentException("Strategy identifier is required.", nameof(strategyId));

        UserId = userId;
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        Status = BacktestStatus.Queued;
    }

    public void MarkRunning() => Status = BacktestStatus.Running;

    public void MarkSucceeded(string resultSummaryJson)
    {
        Status = BacktestStatus.Succeeded;
        ResultSummaryJson = resultSummaryJson;
        FinishedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = BacktestStatus.Failed;
        ResultSummaryJson = reason;
        FinishedAt = DateTime.UtcNow;
    }
}
