using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Application.DTOs;

/// <summary>
/// Payload used during portfolio mutation. We accept the enum directly from the API
/// so the caller can send a strong, validated value instead of raw strings.
/// </summary>
public sealed class AddPositionRequest
{
    public string Symbol { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
}
