using Microsoft.AspNetCore.Mvc;
using PortfolioAnalytics.Application.Abstractions;
using PortfolioAnalytics.Application.Commands;
using PortfolioAnalytics.Application.DTOs;
using PortfolioAnalytics.Application.Handlers;

namespace PortfolioAnalytics.Api.Controllers;

/// <summary>
/// Exposes the authentication endpoints for registration and login.
/// The controller stays thin and delegates the actual process to handlers and services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly LoginUserHandler _loginUserHandler;
    private readonly ITokenService _tokenService;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginUserHandler loginUserHandler,
        ITokenService tokenService)
    {
        _registerUserHandler = registerUserHandler;
        _loginUserHandler = loginUserHandler;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Registers a new account and immediately issues a JWT for the created user.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> RegisterAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _registerUserHandler.HandleAsync(
            new RegisterUserCommand(request.Email, request.FullName, request.Password),
            cancellationToken);

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = _tokenService.GenerateToken(user)
        });
    }

    /// <summary>
    /// Authenticates existing credentials and returns a short-lived access token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _loginUserHandler.HandleAsync(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = _tokenService.GenerateToken(user)
        });
    }
}
