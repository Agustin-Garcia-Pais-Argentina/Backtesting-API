# MVP ToDo - PortfolioAnalytics API

This roadmap combines functionality and feasibility. Priority is ordered by value to the user and by the ability to deliver something useful without overbuilding the foundation.

## Current status (August 2026)

The project foundation is already functional across several key blocks:

- JWT authentication and endpoint protection,
- user registration and login,
- portfolio and position management with domain-based rules,
- MVP market data support with symbol and date-range queries,
- in-memory repositories as a validation layer for the workflow.

What is still not resolved in the functional base is:
- real backtesting over historical series,
- reproducible financial metrics,
- real persistence in PostgreSQL,
- automated domain and integration tests.

## P0 - Product foundation and what makes the MVP useful

### 1. Define the minimum financial domain
- Objective: clarify which entities and business rules are necessary for a portfolio analytics engine.
- Owner: backend + product architect.
- How: define entities such as User, Portfolio, Position, MarketDataPoint, StrategyDefinition, BacktestRun, and PerformanceMetrics.
- Where: `src/PortfolioAnalytics.Domain/`
- Impact: the entire project, because it establishes the base model.
- Future improvements: add Value Objects for Money, DateRange, StrategyParameters, and stricter financial validation rules.

### 2. Create the .NET solution and project structure
- Objective: prepare the repository to scale without mixing responsibilities.
- Owner: backend.
- How: create projects such as `Domain`, `Application`, `Infrastructure`, `Api`, `Worker`, `Contracts`, `Shared`, and `tests`.
- Where: `src/` and `tests/`
- Impact: all code organization and build flow.
- Future improvements: split packages by feature or by context as the project grows and becomes more specialized.

### 3. Configure PostgreSQL and the local environment with Docker
- Objective: enable a real database for development and integration testing.
- Owner: backend / DevOps.
- How: define `docker-compose.yml` with PostgreSQL and minimal environment variables.
- Where: project root and `docker/`
- Impact: local development, tests, and deployment.
- Future improvements: add Redis, pgAdmin, observability, and seed scripts.

### 4. Implement JWT authentication
- Objective: protect user, portfolio, and result endpoints.
- Owner: backend.
- How: build registration/login flow, JWT generation, claim validation, and authentication middleware.
- Where: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Infrastructure/Identity/`
- Impact: security and user access.
- Future improvements: refresh tokens, key rotation, roles, and session auditing.

### 5. Implement CRUD for users and portfolios
- Objective: allow a user to create and manage a portfolio.
- Owner: backend + API.
- How: create portfolio endpoints and validations for name, user, and status.
- Where: `src/PortfolioAnalytics.Application/`, `src/PortfolioAnalytics.Api/Controllers/`
- Impact: the core product use case.
- Future improvements: portfolio sharing, per-user permissions, risk tags, and portfolio snapshots.

### 6. Implement positions inside the portfolio
- Objective: represent assets within a portfolio with quantity and base cost.
- Owner: domain + application.
- How: model `Position` and build services to add, update, and remove positions.
- Where: `src/PortfolioAnalytics.Domain/Entities/` and `src/PortfolioAnalytics.Application/`
- Impact: the main financial functionality.
- Future improvements: support complex instruments, lots, transaction cost handling, and automatic rebalancing.

### 7. Implement historical market data ingestion
- Objective: populate time series to make strategies comparable.
- Owner: application + infrastructure.
- How: create sync, normalization, and deduplication flow by symbol + date + source.
- Where: `src/PortfolioAnalytics.Infrastructure/ExternalServices/`, `src/PortfolioAnalytics.Application/`
- Impact: backtesting and result quality.
- Future improvements: support more providers, source fallback, holiday validation, and data enrichment.

### 8. Create the first backtest strategy
- Objective: demonstrate real value with a useful and testable strategy.
- Owner: domain + application.
- How: start with a simple SMA crossover or buy-and-hold strategy with configurable parameters.
- Where: `src/PortfolioAnalytics.Domain/`, `src/PortfolioAnalytics.Application/Services/`, `src/PortfolioAnalytics.Worker/`
- Impact: the system’s ability to deliver performance analysis.
- Future improvements: include rebalancing strategies, momentum, mean reversion, parameter optimization, and multi-asset backtests.

### 9. Run backtests in the background
- Objective: prevent the API from blocking when a heavy calculation is executed.
- Owner: backend + worker.
- How: an endpoint returns 202 Accepted with a job ID, and a worker processes the calculation.
- Where: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Worker/`, `src/PortfolioAnalytics.Infrastructure/BackgroundJobs/`
- Impact: user experience and API scalability.
- Future improvements: job queues, retries, cancellation, event-driven processing, and external job storage.

### 10. Save backtest results and key metrics
- Objective: persist results for later comparison.
- Owner: infrastructure + application.
- How: store run summary, strategy, parameters, and calculated metrics.
- Where: `src/PortfolioAnalytics.Domain/Entities/`, `src/PortfolioAnalytics.Infrastructure/Repositories/`
- Impact: the usefulness of the product because it allows strategy comparison over time.
- Future improvements: save equity curve series, trade logs, and date-based snapshots.

### 11. Expose MVP REST endpoints
- Objective: make features ready for a frontend or external consumer.
- Owner: API.
- How: build endpoints for login, portfolio, market data, and backtest status/results.
- Where: `src/PortfolioAnalytics.Api/Controllers/`
- Impact: the full usage experience.
- Future improvements: API versioning, pagination, advanced filters, and stable contracts.

## P1 - Product value and feature improvement

### 12. Show performance metrics in a dashboard
- Objective: make results understandable to the user.
- Owner: frontend + API + analytics.
- How: return summary metrics so the client can render graphs.
- Where: `src/PortfolioAnalytics.Api/` and `client/`
- Impact: product adoption and decision-making.
- Future improvements: benchmark comparisons, heatmaps, equity curves, drawdown charts, and CSV/PDF export.

### 13. Compare strategies against each other
- Objective: help users evaluate which strategy performs best on the same portfolio.
- Owner: backend + frontend.
- How: save several runs and compare their metrics.
- Where: `src/PortfolioAnalytics.Application/Queries/` and `src/PortfolioAnalytics.Api/`
- Impact: the analytical usefulness of the product.
- Future improvements: automated parameter optimization and rankings by weighted metrics.

### 14. Implement unit tests for domain and use cases
- Objective: keep business rules stable over time.
- Owner: backend.
- How: use xUnit/NUnit to validate portfolios, positions, and metrics.
- Where: `tests/PortfolioAnalytics.UnitTests/`
- Impact: base quality and regression reduction.
- Future improvements: property-based tests, golden files, and fixtures with real datasets.

### 15. Implement integration tests with real PostgreSQL
- Objective: ensure the database, repositories, and API work together correctly.
- Owner: backend.
- How: use Testcontainers to spin up PostgreSQL during CI.
- Where: `tests/PortfolioAnalytics.IntegrationTests/`
- Impact: confidence in real deployment behavior.
- Future improvements: end-to-end API validation and CI pipelines for migration checks.

### 16. Improve deployment and environment readiness
- Objective: move beyond local-only development.
- Owner: backend + DevOps.
- How: add configuration management, environment variables, Docker Compose, and CI workflows.
- Where: root, `docker/`, and CI config.
- Impact: operational stability and team usability.
- Future improvements: production deployment strategy, health checks, and observability pipeline.

## P2 - Scale and resilience

### 17. Introduce asynchronous processing and queueing
- Objective: support heavier workloads without blocking the API.
- Owner: backend + worker.
- How: standardize job execution and queue handling for slow calculations.
- Where: worker, infrastructure, and background processing modules.
- Impact: performance and scalability.
- Future improvements: retries, dead-letter queues, job cancellation, and metrics collection.

### 18. Add real market-data integrations
- Objective: move from local test data to provider-backed market feeds.
- Owner: infrastructure + application.
- How: implement provider adapters and normalization layers for incoming market data.
- Where: `src/PortfolioAnalytics.Infrastructure/ExternalServices/`
- Impact: product realism and analytic quality.
- Future improvements: multiple providers, backfills, and alerting for failed syncs.

## Implementation guidance

- Keep the MVP focus on real user value, not theoretical architecture.
- Keep the domain model explicit and stable before adding complexity.
- Make each step small enough to validate quickly.
- Use tests as a safety net for critical financial rules.
- Keep documentation aligned with the current state of the project as the code evolves.

## Notes

This project is intended to be a useful financial analysis and strategy platform, not a disconnected architecture demo. The principle is to prioritize pragmatic, Domain-Driven Design (DDD) over theoretical over-engineering.
 confiabilidad del producto.- Mejoras futuras: pruebas end-to-end con cliente y pipeline CI/CD completo.

