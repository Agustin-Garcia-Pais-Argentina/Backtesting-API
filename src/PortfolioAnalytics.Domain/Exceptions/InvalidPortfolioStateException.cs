namespace PortfolioAnalytics.Domain.Exceptions;

public class InvalidPortfolioStateException : Exception
{
    public InvalidPortfolioStateException(string message) : base(message)
    {
    }
}
