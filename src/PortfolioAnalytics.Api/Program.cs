using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PortfolioAnalytics.Api.Exceptions;
using PortfolioAnalytics.Api.Backtesting;
using PortfolioAnalytics.Application.Abstractions;
using PortfolioAnalytics.Application.Handlers;
using PortfolioAnalytics.Application.Services;
using PortfolioAnalytics.Domain.Interfaces;
using PortfolioAnalytics.Domain.Services;
using PortfolioAnalytics.Infrastructure.Identity;
using PortfolioAnalytics.Infrastructure.Repositories;

// This file is the composition root of the API.
// It wires together the infrastructure implementations, the application handlers, and the HTTP pipeline.
var builder = WebApplication.CreateBuilder(args);

// Validate security settings before registering the authentication pipeline. A known
// development key is allowed only for local development; other environments must
// provide their own sufficiently long secret through configuration.
var jwtSettings = JwtSettings.Load(builder.Configuration, builder.Environment);

// Composition root: here we decide which concrete implementations are used.
// This is the point where the API connects the infrastructure with the application layer.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

// In-memory repositories are used for the MVP so we can validate the flow quickly.
// Later, we will replace these with PostgreSQL-backed implementations.
builder.Services.AddSingleton<InMemoryPortfolioRepository>();
builder.Services.AddSingleton<IPortfolioRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryPortfolioRepository>());

builder.Services.AddSingleton<InMemoryUserRepository>();
builder.Services.AddSingleton<IUserRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryUserRepository>());

builder.Services.AddSingleton<InMemoryMarketDataRepository>();
builder.Services.AddSingleton<IMarketDataRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryMarketDataRepository>());

builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services.AddSingleton<CreatePortfolioHandler>();
builder.Services.AddSingleton<AddPositionHandler>();
builder.Services.AddSingleton<GetPortfolioByIdHandler>();
builder.Services.AddSingleton<RegisterUserHandler>();
builder.Services.AddSingleton<LoginUserHandler>();
builder.Services.AddSingleton<SyncMarketDataHandler>();
builder.Services.AddSingleton<GetMarketDataBySymbolHandler>();
builder.Services.AddSingleton<BacktestCalculator>();
builder.Services.AddSingleton<BacktestExecutionStore>();
builder.Services.AddSingleton<BacktestExecutionQueue>();
builder.Services.AddSingleton<RunBacktestHandler>();
builder.Services.AddHostedService<BacktestExecutionWorker>();

// JWT configuration: the API validates the token on every protected request.
// This is how we know which user is calling the endpoint.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var marketDataRepository = app.Services.GetRequiredService<InMemoryMarketDataRepository>();
marketDataRepository.SeedSampleData();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "PortfolioAnalytics.Api"
}));
app.MapControllers();

app.Run();
