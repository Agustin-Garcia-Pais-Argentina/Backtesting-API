using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Domain.Entities;

/// <summary>
/// Represents the user's portfolio aggregate.
/// A portfolio owns positions, identifies the user who created it, and centralizes the rules
/// that keep the collection consistent from the domain perspective.
/// </summary>
public class Portfolio
{
    private readonly object _positionsSyncRoot = new();
    private readonly List<Position> _positions = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public ICollection<Position> Positions
    {
        get
        {
            lock (_positionsSyncRoot)
            {
                return _positions.ToList();
            }
        }
    }

    /// <summary>
    /// The constructor enforces the minimum invariants of a valid portfolio.
    /// We do not allow empty ownership or empty names; the aggregate must remain valid at creation time.
    /// </summary>
    public Portfolio(Guid userId, string name)
    {
        // The domain entity is responsible for protecting itself from invalid states.
        // A portfolio without a valid owner or name should never be created.
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Portfolio name is required.", nameof(name));

        UserId = userId;
        Name = name.Trim();
    }

    // This method is intentionally part of the aggregate: the portfolio decides how it adds positions.
    // That means we prevent duplicate tickers and keep the domain rule close to the data.
    public void AddPosition(Position position)
    {
        if (position is null)
            throw new ArgumentNullException(nameof(position));

        lock (_positionsSyncRoot)
        {
            if (_positions.Any(existing => existing.Symbol == position.Symbol))
                throw new InvalidOperationException($"A position for symbol '{position.Symbol}' already exists in this portfolio.");

            _positions.Add(position);
        }
    }

    public void RemovePosition(string symbol)
    {
        lock (_positionsSyncRoot)
        {
            var position = _positions.FirstOrDefault(x => x.Symbol == symbol);
            if (position is null)
                throw new InvalidOperationException($"No position found for symbol '{symbol}'.");

            _positions.Remove(position);
        }
    }
}
