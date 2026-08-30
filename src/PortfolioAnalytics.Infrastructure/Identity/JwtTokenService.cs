using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PortfolioAnalytics.Application.Abstractions;
using PortfolioAnalytics.Domain.Entities;

namespace PortfolioAnalytics.Infrastructure.Identity;

/// <summary>
/// Produces signed JWT tokens for authenticated users.
/// The token contains identity claims so that the API can authorize requests without storing server-side sessions.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Builds a JWT with the user identity and standard signing metadata.
    /// </summary>
    public string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? "ThisIsATestKeyForLocalDevelopmentOnly_1234567890";
        var issuer = _configuration["Jwt:Issuer"] ?? "PortfolioAnalytics";
        var audience = _configuration["Jwt:Audience"] ?? "PortfolioAnalyticsUsers";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
