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

    public bool Update(Guid id, Func<BacktestRunResponse, BacktestRunResponse> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        while (_runs.TryGetValue(id, out var currentRun))
        {
            var updatedRun = update(currentRun);
            if (_runs.TryUpdate(id, updatedRun, currentRun))
            {
                return true;
            }
        }

        return false;
    }

    public BacktestRunResponse? GetById(Guid id, Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        return _runs.TryGetValue(id, out var run) && run.UserId == userId
            ? run
            : null;
    }

    public IReadOnlyCollection<BacktestRunResponse> GetRecent(Guid userId, int limit = 20)
    {
        if (userId == Guid.Empty)
        {
            return Array.Empty<BacktestRunResponse>();
        }

        return _runs.Values
            .Where(run => run.UserId == userId)
            .OrderByDescending(run => run.CreatedAt)
            .Take(limit)
            .ToList();
    }
}
