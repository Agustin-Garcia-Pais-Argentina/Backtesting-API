using System.Collections.Concurrent;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Infrastructure.Repositories;

/// <summary>
/// Temporary in-memory implementation of the user repository.
/// It is sufficient for MVP validation because it allows the system to behave like a real repository
/// without introducing database setup too early in the learning process.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _usersById = new();
    private readonly ConcurrentDictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncRoot = new();

    /// <summary>
    /// Stores a user by both identifier and normalized email.
    /// </summary>
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var normalizedEmail = user.Email.Trim();
            if (_usersByEmail.ContainsKey(normalizedEmail))
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            if (_usersById.ContainsKey(user.Id))
            {
                throw new InvalidOperationException("A user with this identifier already exists.");
            }

            _usersById[user.Id] = user;
            _usersByEmail[normalizedEmail] = user;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a user by exact email lookup while ignoring case.
    /// </summary>
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_usersByEmail.TryGetValue(email.Trim(), out var user) ? user : null);
        }
    }

    /// <summary>
    /// Retrieves a user by internal identifier.
    /// </summary>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_usersById.TryGetValue(id, out var user) ? user : null);
        }
    }
}
