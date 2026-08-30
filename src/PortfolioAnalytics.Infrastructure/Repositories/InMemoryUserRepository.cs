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
    private readonly Dictionary<Guid, User> _usersById = new();
    private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Stores a user by both identifier and normalized email.
    /// </summary>
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _usersById[user.Id] = user;
        _usersByEmail[user.Email] = user;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a user by exact email lookup while ignoring case.
    /// </summary>
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_usersByEmail.TryGetValue(email.Trim(), out var user) ? user : null);
    }

    /// <summary>
    /// Retrieves a user by internal identifier.
    /// </summary>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_usersById.TryGetValue(id, out var user) ? user : null);
    }
}
