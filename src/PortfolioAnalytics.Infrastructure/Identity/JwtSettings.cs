using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PortfolioAnalytics.Infrastructure.Identity;

/// <summary>
/// Contains the validated settings used to issue and validate JWTs.
/// </summary>
public sealed class JwtSettings
{
    private const string DevelopmentFallbackKey = "ThisIsATestKeyForLocalDevelopmentOnly_1234567890";

    /// <summary>
    /// Gets the minimum length accepted for an HMAC signing key.
    /// </summary>
    public const int MinimumKeyLength = 32;

    /// <summary>
    /// Gets the signing key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the token issuer.
    /// </summary>
    public string Issuer { get; }

    /// <summary>
    /// Gets the token audience.
    /// </summary>
    public string Audience { get; }

    private JwtSettings(string key, string issuer, string audience)
    {
        Key = key;
        Issuer = issuer;
        Audience = audience;
    }

    /// <summary>
    /// Loads and validates JWT settings from configuration.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="environment">The current host environment.</param>
    /// <returns>Validated JWT settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a non-Development environment has no valid signing key.
    /// </exception>
    public static JwtSettings Load(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        return Load(configuration, environment.IsDevelopment());
    }

    /// <summary>
    /// Loads and validates JWT settings for the specified environment name.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="environmentName">The current host environment name.</param>
    /// <returns>Validated JWT settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a non-Development environment has no valid signing key.
    /// </exception>
    public static JwtSettings Load(IConfiguration configuration, string environmentName)
    {
        return Load(
            configuration,
            string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase));
    }

    private static JwtSettings Load(IConfiguration configuration, bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            if (!isDevelopment)
            {
                throw new InvalidOperationException(
                    $"Jwt:Key must be configured outside Development and be at least {MinimumKeyLength} characters long.");
            }

            key = DevelopmentFallbackKey;
        }

        if (key.Length < MinimumKeyLength)
        {
            throw new InvalidOperationException(
                $"Jwt:Key must be at least {MinimumKeyLength} characters long.");
        }

        if (!isDevelopment && string.Equals(key, DevelopmentFallbackKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Jwt:Key must not use the Development fallback outside Development.");
        }

        var issuer = configuration["Jwt:Issuer"] ?? "PortfolioAnalytics";
        var audience = configuration["Jwt:Audience"] ?? "PortfolioAnalyticsUsers";

        return new JwtSettings(key, issuer, audience);
    }
}
