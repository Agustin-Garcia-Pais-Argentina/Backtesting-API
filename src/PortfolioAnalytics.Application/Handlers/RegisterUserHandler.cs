using PortfolioAnalytics.Application.Abstractions;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Domain.Entities;
using PortfolioAnalytics.Domain.Interfaces;

namespace PortfolioAnalytics.Application.Handlers;

/// <summary>
/// Handles the registration use case.
/// This is where email validation, duplicate checks, and password hashing are coordinated before persistence.
/// </summary>
public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Creates a new user only after the required inputs are valid and the email is not already in use.
    /// </summary>
    public async Task<User> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email is required.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ArgumentException("Password is required.", nameof(command));

        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User(
            command.Email,
            command.FullName,
            _passwordHasher.Hash(command.Password));

        await _userRepository.AddAsync(user, cancellationToken);
        return user;
    }
}
