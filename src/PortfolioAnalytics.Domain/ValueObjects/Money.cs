namespace PortfolioAnalytics.Domain.ValueObjects;

public readonly record struct Money(decimal Amount)
{
    public static Money Zero => new(0m);

    public override string ToString() => Amount.ToString("C");
}
