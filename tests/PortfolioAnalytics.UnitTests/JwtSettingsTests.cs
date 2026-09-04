using Microsoft.Extensions.Configuration;
using PortfolioAnalytics.Infrastructure.Identity;

namespace PortfolioAnalytics.UnitTests;

public class JwtSettingsTests
{
    [Fact]
    public void Load_UsesFallbackOnlyInDevelopment()
    {
        var settings = JwtSettings.Load(CreateConfiguration(), "Development");

        Assert.True(settings.Key.Length >= JwtSettings.MinimumKeyLength);
    }

    [Fact]
    public void Load_RejectsMissingKeyOutsideDevelopment()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => JwtSettings.Load(CreateConfiguration(), "Production"));

        Assert.Contains("Jwt:Key must be configured outside Development", exception.Message);
        Assert.DoesNotContain("ThisIsATestKeyForLocalDevelopmentOnly", exception.Message);
    }

    [Fact]
    public void Load_RejectsWeakConfiguredKey()
    {
        var configuration = CreateConfiguration(("Jwt:Key", new string('x', JwtSettings.MinimumKeyLength - 1)));

        var exception = Assert.Throws<InvalidOperationException>(
            () => JwtSettings.Load(configuration, "Production"));

        Assert.Contains("at least 32 characters", exception.Message);
    }

    [Fact]
    public void Load_RejectsDevelopmentFallbackOutsideDevelopment()
    {
        var developmentSettings = JwtSettings.Load(CreateConfiguration(), "Development");
        var configuration = CreateConfiguration(("Jwt:Key", developmentSettings.Key));

        var exception = Assert.Throws<InvalidOperationException>(
            () => JwtSettings.Load(configuration, "Production"));

        Assert.Contains("must not use the Development fallback", exception.Message);
    }

    [Fact]
    public void Load_ReturnsConfiguredValues()
    {
        var configuration = CreateConfiguration(
            ("Jwt:Key", new string('x', JwtSettings.MinimumKeyLength)),
            ("Jwt:Issuer", "ConfiguredIssuer"),
            ("Jwt:Audience", "ConfiguredAudience"));

        var settings = JwtSettings.Load(configuration, "Production");

        Assert.Equal(new string('x', JwtSettings.MinimumKeyLength), settings.Key);
        Assert.Equal("ConfiguredIssuer", settings.Issuer);
        Assert.Equal("ConfiguredAudience", settings.Audience);
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        var builder = new ConfigurationBuilder();
        foreach (var value in values)
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [value.Key] = value.Value
            });
        }

        return builder.Build();
    }
}
