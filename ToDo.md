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

## MVP completion checklist before calling the project "useful and working"

This is the exact sequence we should close before declaring the MVP ready for a meaningful demo or public-facing use.

### 1. Validate the end-to-end user journey — **DONE**
- Objective: confirm that the main product flow works without manual corrections.
- Sequence: register user -> login -> create portfolio -> add positions -> fetch market data -> run backtest -> retrieve result.
- Success condition: a user can complete the entire flow in a real run without patching the application or changing the contract ad hoc.
- Status: completed for the happy path. The core flow was exercised successfully in a real local run and the MVP flow is operational for valid requests.
- Future improvements: add richer UX flows, dashboards, and additional portfolio operations.

### 2. Stabilize the API contract — **DONE**
- Objective: keep the request and response models clear, consistent, and documented.
- How: review route naming, DTOs, status codes, and payload examples before moving into UI work.
- Success condition: Swagger reflects the actual behavior and the response shape is predictable for clients.
- Status: completed. The API contract was reviewed endpoint by endpoint and validated through the Postman happy and negative flows.
- Future improvements: versioning, pagination, filtering, and stronger request validation.

### 3. Tighten validation and error handling — **DONE**
- Objective: make invalid scenarios clear and explainable.
- Required checks: invalid dates, empty symbol ranges, duplicate portfolio logic, invalid capital values, and unauthorized requests.
- Success condition: a client gets accurate errors without ambiguous backend exceptions.
- Future improvements: structured error envelopes, standardized API error codes, and logging/traceability.

### 3.1. Confirm that HTTP error responses are mapped correctly — **DONE**
- Objective: ensure that all business-rule failures and invalid client inputs result in the correct HTTP status code and a predictable payload.
- Required checks: duplicate user registration should return a conflict/error response instead of an unhandled exception, invalid input should return 400/422, unauthorized access should return 401, and not-found cases should return 404.
- Success condition: the API responds consistently and clearly for every invalid flow a client is expected to hit.
- Status: completed for the web API layer through a centralized exception middleware that translates exceptions into consistent problem-details responses without leaking internal implementation details.
- Future improvements: refine the exact payload contract and document the standard error envelope consistently for all endpoints.

### 3.2. Align Postman negative tests with the real API contract — **DONE**
- Objective: prevent false failures caused by expecting success from flows that are intentionally invalid.
- Required checks: duplicate-email registration must assert a non-200 error, requests without a valid JWT must assert 401, missing or invalid route parameters must assert 400/404, and payloads that do not match the API contract must assert validation failures instead of expecting a successful response.
- Success condition: the tests validate the correct business behavior and the correct HTTP semantics, instead of asserting success for scenarios that are supposed to fail.
- Status: completed. The negative Postman cases were finalized and validated against the expected statuses.
- Future improvements: create a dedicated negative-test collection with explicit expected statuses for duplicate registration, invalid ids, unauthorized access, invalid form bodies, and empty time ranges.

### 4. Document local configuration and startup procedure — **DONE**
- Objective: ensure the project can be run by another engineer or reviewer without hidden setup steps.
- Required items: environment variables, JWT config, startup commands, sample data behavior, and local API calls.
- Success condition: a contributor can run the app locally using the project instructions and reproduce the main flow.
- Status: completed. README documents startup, JWT configuration, Swagger, health checks, sample data, and the reproducible API flow.
- Future improvements: Docker Compose automation, environment profiles, and deployment-ready configuration.

### 5. Decide the persistence strategy explicitly — **DONE**
- Objective: clarify whether the current in-memory store is temporary or part of the real MVP contract.
- Required decision: keep in-memory repositories only as an intentional MVP stage, and plan PostgreSQL + EF Core as the next real persistence milestone.
- Success condition: the architecture document and the README clearly explain the reasoning.
- Future improvements: migrations, repository implementation, transactions, and durable data storage.

### 6. Confirm the MVP is demo-ready with deterministic sample data — **DONE**
- Objective: avoid depending on external providers too early.
- How: keep a minimal, repeatable dataset and a predictable run that can be re-used for demos and testing.
- Success condition: the project can be demonstrated without brittle or flaky external dependencies.
- Future improvements: real market-data ingestion, provider integrations, and normalization pipelines.

### 7. Add basic operational checks — **DONE**
- Objective: make the app easier to run, diagnose, and trust.
- Required items: a health or status endpoint, clear logs for failed requests, and a minimal note in the README about the demo flow.
- Success condition: a developer can tell whether the app is healthy and whether a failure is expected or a real bug.
- Status: completed. `/health` provides a basic liveness check, the middleware logs unhandled failures, and README documents the demo flow.
- Future improvements: observability, dashboards, tracing, and performance metrics.

## What comes after the MVP: product, technical, and integration improvements

Once the project is already useful and stable, the remaining work shifts from "basic functionality" to "scaling and quality".

### Product improvements — **IN QUEUE**
- portfolio dashboard and historical results
- charting for drawdown, equity curve, and performance snapshots
- saved strategies and parameter presets
- better comparison between strategy runs
- portfolio rebalancing and watchlists

### Technical improvements — **IN QUEUE**
- PostgreSQL + EF Core persistence
- repository and unit-of-work cleanup
- background job queues for expensive simulations
- integration testing with real database and API validation
- CI/CD, deployment pipelines, and environment automation

### External integrations — **IN QUEUE**
- real market-data providers
- broker or exchange APIs
- authentication providers or enterprise identity integration
- notifications, alerts, and reporting flows

## P0 - Product foundation and what makes the MVP useful

### 1. Define the minimum financial domain — **DONE**
- Objective: clarify which entities and business rules are necessary for a portfolio analytics engine.
- Owner: backend + product architect.
- How: define entities such as User, Portfolio, Position, MarketDataPoint, StrategyDefinition, BacktestRun, and PerformanceMetrics.
- Where: `src/PortfolioAnalytics.Domain/`
- Impact: the entire project, because it establishes the base model.
- Future improvements: add Value Objects for Money, DateRange, StrategyParameters, and stricter financial validation rules.

### 2. Create the .NET solution and project structure — **DONE**
- Objective: prepare the repository to scale without mixing responsibilities.
- Owner: backend.
- How: create projects such as `Domain`, `Application`, `Infrastructure`, `Api`, `Worker`, `Contracts`, `Shared`, and `tests`.
- Where: `src/` and `tests/`
- Impact: all code organization and build flow.
- Future improvements: split packages by feature or by context as the project grows and becomes more specialized.

### 3. Configure PostgreSQL and the local environment with Docker — **NEXT IN LINE**
- Objective: enable a real database for development and integration testing.
- Owner: backend / DevOps.
- How: define `docker-compose.yml` with PostgreSQL and minimal environment variables.
- Where: project root and `docker/`
- Impact: local development, tests, and deployment.
- Future improvements: add Redis, pgAdmin, observability, and seed scripts.

### 4. Implement JWT authentication — **DONE**
- Objective: protect user, portfolio, and result endpoints.
- Owner: backend.
- How: build registration/login flow, JWT generation, claim validation, and authentication middleware.
- Where: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Infrastructure/Identity/`
- Impact: security and user access.
- Future improvements: refresh tokens, key rotation, roles, and session auditing.

### 5. Implement CRUD for users and portfolios — **DONE**
- Objective: allow a user to create and manage a portfolio.
- Owner: backend + API.
- How: create portfolio endpoints and validations for name, user, and status.
- Where: `src/PortfolioAnalytics.Application/`, `src/PortfolioAnalytics.Api/Controllers/`
- Impact: the core product use case.
- Future improvements: portfolio sharing, per-user permissions, risk tags, and portfolio snapshots.

### 6. Implement positions inside the portfolio — **DONE**
- Objective: represent assets within a portfolio with quantity and base cost.
- Owner: domain + application.
- How: model `Position` and build services to add, update, and remove positions.
- Where: `src/PortfolioAnalytics.Domain/Entities/` and `src/PortfolioAnalytics.Application/`
- Impact: the main financial functionality.
- Future improvements: support complex instruments, lots, transaction cost handling, and automatic rebalancing.

### 7. Implement historical market data ingestion — **DONE**
- Objective: populate time series to make strategies comparable.
- Owner: application + infrastructure.
- How: create sync, normalization, and deduplication flow by symbol + date + source.
- Where: `src/PortfolioAnalytics.Infrastructure/ExternalServices/`, `src/PortfolioAnalytics.Application/`
- Impact: backtesting and result quality.
- Future improvements: support more providers, source fallback, holiday validation, and data enrichment.

### 8. Create the first backtest strategy — **DONE**
- Objective: demonstrate real value with a useful and testable strategy.
- Owner: domain + application.
- How: start with a simple SMA crossover or buy-and-hold strategy with configurable parameters.
- Where: `src/PortfolioAnalytics.Domain/`, `src/PortfolioAnalytics.Application/Services/`, `src/PortfolioAnalytics.Worker/`
- Impact: the system’s ability to deliver performance analysis.
- Future improvements: include rebalancing strategies, momentum, mean reversion, parameter optimization, and multi-asset backtests.

### 9. Run backtests in the background — **IN QUEUE**
- Objective: prevent the API from blocking when a heavy calculation is executed.
- Owner: backend + worker.
- How: an endpoint returns 202 Accepted with a job ID, and a worker processes the calculation.
- Where: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Worker/`, `src/PortfolioAnalytics.Infrastructure/BackgroundJobs/`
- Impact: user experience and API scalability.
- Future improvements: job queues, retries, cancellation, event-driven processing, and external job storage.

### 10. Save backtest results and key metrics — **IN QUEUE**
- Objective: persist results for later comparison.
- Owner: infrastructure + application.
- How: store run summary, strategy, parameters, and calculated metrics.
- Where: `src/PortfolioAnalytics.Domain/Entities/`, `src/PortfolioAnalytics.Infrastructure/Repositories/`
- Impact: the usefulness of the product because it allows strategy comparison over time.
- Future improvements: save equity curve series, trade logs, and date-based snapshots.

### 11. Expose MVP REST endpoints — **DONE**
- Objective: make features ready for a frontend or external consumer.
- Owner: API.
- How: build endpoints for login, portfolio, market data, and backtest status/results.
- Where: `src/PortfolioAnalytics.Api/Controllers/`
- Impact: the full usage experience.
- Future improvements: API versioning, pagination, advanced filters, and stable contracts.

## P1 - Product value and feature improvement

### 12. Show performance metrics in a dashboard — **IN QUEUE**
- Objective: make results understandable to the user.
- Owner: frontend + API + analytics.
- How: return summary metrics so the client can render graphs.
- Where: `src/PortfolioAnalytics.Api/` and `client/`
- Impact: product adoption and decision-making.
- Future improvements: benchmark comparisons, heatmaps, equity curves, drawdown charts, and CSV/PDF export.

### 13. Compare strategies against each other — **IN QUEUE**
- Objective: help users evaluate which strategy performs best on the same portfolio.
- Owner: backend + frontend.
- How: save several runs and compare their metrics.
- Where: `src/PortfolioAnalytics.Application/Queries/` and `src/PortfolioAnalytics.Api/`
- Impact: the analytical usefulness of the product.
- Future improvements: automated parameter optimization and rankings by weighted metrics.

### 14. Implement unit tests for domain and use cases — **DONE**
- Objective: keep business rules stable over time.
- Owner: backend.
- How: use xUnit/NUnit to validate portfolios, positions, and metrics.
- Where: `tests/PortfolioAnalytics.UnitTests/`
- Impact: base quality and regression reduction.
- Future improvements: property-based tests, golden files, and fixtures with real datasets.

### 15. Implement integration tests with real PostgreSQL — **IN QUEUE**
- Objective: ensure the database, repositories, and API work together correctly.
- Owner: backend.
- How: use Testcontainers to spin up PostgreSQL during CI.
- Where: `tests/PortfolioAnalytics.IntegrationTests/`
- Impact: confidence in real deployment behavior.
- Future improvements: end-to-end API validation and CI pipelines for migration checks.

### 16. Improve deployment and environment readiness — **IN QUEUE**
- Objective: move beyond local-only development.
- Owner: backend + DevOps.
- How: add configuration management, environment variables, Docker Compose, and CI workflows.
- Where: root, `docker/`, and CI config.
- Impact: operational stability and team usability.
- Future improvements: production deployment strategy, health checks, and observability pipeline.

## P2 - Scale and resilience

### 17. Introduce asynchronous processing and queueing — **IN QUEUE**
- Objective: support heavier workloads without blocking the API.
- Owner: backend + worker.
- How: standardize job execution and queue handling for slow calculations.
- Where: worker, infrastructure, and background processing modules.
- Impact: performance and scalability.
- Future improvements: retries, dead-letter queues, job cancellation, and metrics collection.

### 18. Add real market-data integrations — **IN QUEUE**
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
