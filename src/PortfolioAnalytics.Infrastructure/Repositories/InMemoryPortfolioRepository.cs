using System.Collections.Concurrent;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Infrastructure.Repositories;

/// <summary>
/// This in-memory repository is intentionally simple and temporary.
/// It gives us a working persistence layer for the MVP without forcing EF Core setup yet.
/// The important thing for learning is that the repository hides storage details behind a contract.
/// </summary>
public sealed class InMemoryPortfolioRepository : IPortfolioRepository
{
    private readonly ConcurrentDictionary<Guid, Portfolio> _portfolios = new();
    private readonly object _syncRoot = new();

    public Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            _portfolios[portfolio.Id] = portfolio;
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            _portfolios[portfolio.Id] = portfolio;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            _portfolios.TryRemove(id, out _);
        }

        return Task.CompletedTask;
    }

    public Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_portfolios.TryGetValue(id, out var portfolio) ? portfolio : null);
        }
    }

    public Task<IEnumerable<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var userPortfolios = _portfolios.Values
                .Where(portfolio => portfolio.UserId == userId)
                .ToList();
            return Task.FromResult<IEnumerable<Portfolio>>(userPortfolios);
        }
    }
}
