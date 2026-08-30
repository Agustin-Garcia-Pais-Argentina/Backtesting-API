using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Domain.Entities;

/// <summary>
/// Represents an individual asset position inside a portfolio.
/// The entity stores the identifier of the asset, how much is owned, and the average acquisition cost.
/// </summary>
public class Position
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PortfolioId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public AssetType AssetType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal AverageCost { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the invariants for a safe position creation.
    /// In a financial system, a position without quantity or cost is not meaningful.
    /// </summary>
    public Position(Guid portfolioId, string symbol, AssetType assetType, decimal quantity, decimal averageCost)
    {
        if (portfolioId == Guid.Empty)
            throw new ArgumentException("Portfolio identifier is required.", nameof(portfolioId));

        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol is required.", nameof(symbol));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (averageCost <= 0)
            throw new ArgumentException("Average cost must be greater than zero.", nameof(averageCost));

        PortfolioId = portfolioId;
        Symbol = symbol.Trim().ToUpperInvariant();
        AssetType = assetType;
        Quantity = quantity;
        AverageCost = averageCost;
    }
}
