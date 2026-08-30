using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Application.Commands;

/// <summary>
/// This command represents adding a new asset to an existing portfolio.
/// We pass the raw values to the handler and let the domain object validate them.
/// </summary>
public sealed record AddPositionCommand(
    Guid PortfolioId,
    string Symbol,
    AssetType AssetType,
    decimal Quantity,
    decimal AverageCost);
