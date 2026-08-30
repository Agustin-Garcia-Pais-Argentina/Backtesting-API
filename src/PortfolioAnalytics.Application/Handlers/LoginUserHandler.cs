using PortfolioAnalytics.Application.Abstractions;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>
/// Validates user credentials and returns the authenticated entity when the password matches the stored hash.
/// The handler intentionally does not generate a token; token creation belongs to the infrastructure/auth layer.
/// </summary>
public sealed class LoginUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Looks up the user by email, verifies the password hash, and returns the user when credentials are valid.
    /// </summary>
    public async Task<User> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken)
            ?? throw new InvalidOperationException("Invalid credentials.");

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid credentials.");

        return user;
    }
}
