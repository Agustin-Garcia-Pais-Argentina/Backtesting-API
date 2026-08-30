namespace PortfolioAnalytics.Domain.Entities;

/// <summary>
/// Represents the authenticated account that owns portfolios and performs actions in the platform.
/// The entity keeps the minimum identity information required for authorization and ownership tracking.
/// </summary>
public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public ICollection<Portfolio> Portfolios { get; private set; } = new List<Portfolio>();

    /// <summary>
    /// Constructs a valid user only when the required identity fields are present.
    /// We deliberately store the password hash instead of the password itself.
    /// </summary>
    public User(string email, string fullName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        Email = email.Trim();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
    }
}
