using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace PortfolioAnalytics.Api.Exceptions;

/// <summary>
/// Converts unhandled application and domain exceptions into consistent HTTP problem responses.
/// This keeps the controllers thin and makes the public API contract explicit.
/// </summary>
public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = MapException(exception);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        return context.Response.WriteAsJsonAsync(problem);
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        if (exception is UnauthorizedAccessException or SecurityException or SecurityTokenException)
        {
            return (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication failed.");
        }

        if (exception is ArgumentException)
        {
            return (StatusCodes.Status400BadRequest, "Invalid request", "The request payload is invalid.");
        }

        if (exception is InvalidOperationException)
        {
            var message = exception.Message;

            if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return (StatusCodes.Status409Conflict, "Conflict", "The resource already exists.");
            }

            if (message.Contains("Invalid credentials", StringComparison.OrdinalIgnoreCase))
            {
                return (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication failed.");
            }

            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("No market data available", StringComparison.OrdinalIgnoreCase))
            {
                return (StatusCodes.Status404NotFound, "Resource not found", "The requested resource was not found.");
            }

            return (StatusCodes.Status422UnprocessableEntity, "Business rule violation", "The request could not be processed.");
        }

        return (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.");
    }
}
