using PortfolioAnalytics.Domain.Enums;

namespace PortfolioAnalytics.Domain.Entities;

public class StrategyDefinition
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public StrategyType Type { get; private set; }
    public string ParametersJson { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public StrategyDefinition(string name, StrategyType type, string parametersJson)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Strategy name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(parametersJson))
            throw new ArgumentException("Parameters are required.", nameof(parametersJson));

        Name = name.Trim();
        Type = type;
        ParametersJson = parametersJson;
    }
}
