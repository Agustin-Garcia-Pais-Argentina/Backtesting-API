using System.Collections.Concurrent;
using PortfolioAnalytics.Application.DTOs;

namespace PortfolioAnalytics.Application.Services;

/// <summary>
/// Stores the last backtest results in memory so the API can return them by identifier.
/// This keeps the MVP simple while still exposing a real retrieval workflow.
/// </summary>
public sealed class BacktestExecutionStore
{
    private readonly ConcurrentDictionary<Guid, BacktestRunResponse> _runs = new();

    public BacktestRunResponse Save(BacktestRunResponse run)
    {
        _runs[run.Id] = run;
        return run;
    }

    public BacktestRunResponse? GetById(Guid id)
    {
        _runs.TryGetValue(id, out var run);
        return run;
    }

    public IReadOnlyCollection<BacktestRunResponse> GetRecent(int limit = 20)
    {
        return _runs.Values
            .OrderByDescending(run => run.CreatedAt)
            .Take(limit)
            .ToList();
    }
}
