using System.Collections.Concurrent;
using PortfolioAnalytics.Application.DTOs;

namespace PortfolioAnalytics.Application.Services;

/// <summary>
/// Stores backtest runs and their calculated metrics in memory so they can be queried
/// after background execution completes.
/// </summary>
public sealed class BacktestExecutionStore
{
    private readonly ConcurrentDictionary<Guid, BacktestRunResponse> _runs = new();

    public BacktestRunResponse Save(BacktestRunResponse run)
    {
        _runs[run.Id] = run;
        return run;
    }

    public bool Update(Guid id, Action<BacktestRunResponse> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (!_runs.TryGetValue(id, out var run))
        {
            return false;
        }

        update(run);
        return true;
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
