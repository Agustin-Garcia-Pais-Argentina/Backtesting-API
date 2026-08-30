using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Application.DTOs;

public sealed class PositionResponse
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class PortfolioResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PositionResponse> Positions { get; set; } = new();
}
