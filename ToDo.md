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

## FIX NOW - API security, correctness, and resilience

These fixes must be completed before exposing the API outside a trusted local development environment. They address issues found during the .NET API review and take priority over new product features.

### FIX NOW 1. Isolate backtests by authenticated user — **DONE**
- Objective: prevent one authenticated user from listing or reading backtest runs created by another user.
- What to do: associate every `BacktestRunResponse` and queued `BacktestWorkItem` with the `UserId` extracted from the JWT. Filter recent runs by that owner and return not found when a requested run belongs to another user.
- How to do it: add `UserId` to the backtest request/response flow, pass it from `BacktestsController` into the command and queue item, and update `BacktestExecutionStore.GetRecent` and `GetById` to require the current user identifier. Preserve the existing portfolio ownership pattern so cross-user resources are not enumerable.
- Where: `src/PortfolioAnalytics.Api/Controllers/BacktestsController.cs`, `src/PortfolioAnalytics.Application/Commands/`, `src/PortfolioAnalytics.Application/DTOs/`, `src/PortfolioAnalytics.Application/Services/BacktestExecutionStore.cs`, and `src/PortfolioAnalytics.Application/Services/BacktestExecutionQueue.cs`.
- Status: completed. The controller extracts `ClaimTypes.NameIdentifier`, stores it on the response, command, and queue item, and requires the same owner for recent and individual run reads. Requests with a missing or invalid identity are unauthorized, while another user's run is indistinguishable from a missing run (`404 Not Found`).
- Validation: focused unit tests prove owner filtering for queued, running, completed, and failed backtests and verify that queue items carry the authenticated owner.

### FIX NOW 2. Remove the known fallback JWT key outside Development — **DONE**
- Objective: ensure a deployment cannot run with a publicly known signing secret.
- What to do: keep a development-only local fallback if desired, but fail application startup in non-Development environments when `Jwt:Key` is missing or too weak. Require a sufficiently long secret supplied through configuration or an environment variable.
- How to do it: validate JWT settings during service registration, use `IHostEnvironment` to distinguish Development from other environments, and configure production values through `Jwt__Key`, `Jwt__Issuer`, and `Jwt__Audience`. Do not log the secret.
- Where: `src/PortfolioAnalytics.Api/Program.cs`, `src/PortfolioAnalytics.Infrastructure/Identity/JwtSettings.cs`, `src/PortfolioAnalytics.Infrastructure/Identity/JwtTokenService.cs`, `tests/PortfolioAnalytics.UnitTests/JwtSettingsTests.cs`, `README.md`, and `.env.example`.
- Status: completed. Startup loads one validated `JwtSettings` instance. Development may use the local fallback, while every other environment fails before service registration when `Jwt:Key` is missing or shorter than 32 characters. Authentication and token generation use the same validated settings, and no secret is logged.
- Validation: focused unit tests cover the Development fallback, missing/weak non-Development keys, and valid configured settings; the API build also verifies composition-root wiring.

### FIX NOW 3. Make singleton in-memory repositories thread-safe — **DONE**
- Objective: prevent data races and collection corruption when concurrent HTTP requests access the singleton repositories.
- What to do: replace mutable `Dictionary` instances with `ConcurrentDictionary`, or protect compound reads and writes with a synchronization strategy. Keep returned collections as snapshots so callers cannot enumerate a collection while it is being modified.
- How to do it: apply the same approach consistently to users, portfolios, and market data; review duplicate-check-then-insert flows because they must be atomic or explicitly documented as MVP limitations.
- Where: `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryUserRepository.cs`, `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryPortfolioRepository.cs`, and `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryMarketDataRepository.cs`.
- Status: completed. Singleton repositories now use concurrent dictionaries, lock compound user writes and portfolio repository operations, and return materialized query snapshots. Portfolio position duplicate-check/add is synchronized inside the aggregate so concurrent read-modify-write requests do not corrupt the position collection. Uniqueness is guaranteed only within this process; the PostgreSQL unique index remains the durable invariant.
- Validation: focused xUnit tests cover concurrent user registration attempts, portfolio position updates, and market-data upserts/snapshots; the existing unit suite remains green.

### FIX NOW 4. Bound the backtest queue and honor request cancellation — **DONE**
- Objective: prevent unlimited queued work from exhausting memory and avoid accepting work after the client request has been cancelled.
- What to do: configure a bounded channel with an explicit capacity, return a clear overload response when the queue is full, and pass the controller cancellation token to enqueue instead of `CancellationToken.None`.
- How to do it: use a bounded `Channel<BacktestWorkItem>` with a defined full-mode policy, handle the failed write in the controller, and preserve cancellation behavior in the background worker. Add rate limiting if the endpoint will be publicly reachable.
- Where: `src/PortfolioAnalytics.Application/Services/BacktestExecutionQueue.cs`, `src/PortfolioAnalytics.Api/Controllers/BacktestsController.cs`, and `src/PortfolioAnalytics.Api/Backtesting/BacktestExecutionWorker.cs`.
- Status: completed. The in-process queue uses a bounded channel with an explicit capacity of 100 and immediate full detection. The controller passes its request token to enqueue, removes the provisional run when enqueue is cancelled or overloaded, returns `503 Service Unavailable` when full, and leaves the worker's shutdown cancellation behavior unchanged.
- Validation: focused xUnit tests cover queue saturation, cancelled requests, accepted queued responses, and worker transition to `Completed`. Queue state remains process-local and is lost on restart; a PostgreSQL-backed durable queue/run store is the fallback before multi-instance or production deployment.

### FIX NOW 5. Enforce complete OHLCV financial-data validation — **PENDING**
- Objective: reject invalid market data before it can contaminate backtests and financial metrics.
- What to do: require positive Open, High, Low, and Close prices; require non-negative volume; require `Low <= Open <= High` and `Low <= Close <= High`; and reject non-finite or otherwise invalid numeric values if the input type is later changed to floating point.
- How to do it: centralize these invariants in the `MarketDataPoint` domain constructor so every ingestion path receives the same protection, then map the resulting validation exceptions to the established 400/422 contract.
- Where: `src/PortfolioAnalytics.Domain/Entities/MarketDataPoint.cs` and related market-data request/handler tests.
- Validation: add tests for each invalid OHLCV combination and for a valid boundary case.

### FIX NOW 6. Reject reversed market-data date ranges — **PENDING**
- Objective: keep the market-data API contract consistent by rejecting `from` dates later than `to`.
- What to do: return `400 Bad Request` with the standard error shape when the range is reversed, rather than returning `200 OK` with an empty collection.
- How to do it: after parsing both query parameters in `MarketDataController`, check `fromDate > toDate` and use the same validation response used by the other invalid request cases. Add the condition to the negative API/Postman test collection.
- Where: `src/PortfolioAnalytics.Api/Controllers/MarketDataController.cs` and API contract tests.
- Validation: cover equal dates, a valid ascending range, and a reversed range.

### FIX NOW 7. Add bounded HTTP payload and workload limits — **PENDING**
- Problem: market-data ingestion accepts an unbounded enumerable, and backtest requests do not limit the date range or the amount of pending work. An authenticated client could consume excessive memory or CPU even without exploiting a lower-level vulnerability.
- Objective: keep request processing and background workload within explicit MVP operating limits.
- Solution: define maximum market-data points per request, maximum symbol/source lengths, maximum backtest date range, and maximum pending jobs. Reject invalid sizes with the standard `400 Bad Request` problem-details response and use overload protection for the queue.
- Recommendation: configure limits through strongly typed options instead of hardcoding them in controllers, document the values in Swagger/README, and add rate limiting before exposing the API publicly.
- Where: `src/PortfolioAnalytics.Api/Controllers/MarketDataController.cs`, `src/PortfolioAnalytics.Api/Controllers/BacktestsController.cs`, request DTOs, queue configuration, and API configuration.
- Validation: test requests at the boundary, just above the boundary, and queue saturation behavior.

### FIX NOW 8. Enforce invariant ISO date parsing in market-data queries — **PENDING**
- Problem: the API error message promises `yyyy-MM-dd`, but `DateOnly.TryParse` uses culture-sensitive parsing. The same request can therefore be interpreted differently depending on the server culture.
- Objective: make the HTTP date contract deterministic across environments.
- Solution: parse `from` and `to` with `DateOnly.TryParseExact`, `yyyy-MM-dd`, and `CultureInfo.InvariantCulture`, then keep the existing range validation.
- Recommendation: publish the exact format in the OpenAPI schema and cover malformed, culture-sensitive, equal, ascending, and reversed ranges in contract tests.
- Where: `src/PortfolioAnalytics.Api/Controllers/MarketDataController.cs` and API contract tests.
- Validation: run the same requests under at least two cultures and confirm identical results.

### FIX NOW 9. Make user registration uniqueness atomic — **PENDING**
- Problem: registration performs a check-then-insert sequence. Concurrent requests for the same email can both pass the duplicate check; the in-memory dictionaries are also not safe for concurrent access.
- Objective: guarantee one account per normalized email under concurrent requests.
- Solution: enforce uniqueness in the persistence layer and expose a conflict result mapped to `409 Conflict`. For the temporary in-memory implementation, make the compound operation atomic and normalize the email consistently before lookup and storage.
- Recommendation: preserve the same invariant with a unique database index when PostgreSQL is introduced; add a concurrent registration test.
- Where: `src/PortfolioAnalytics.Application/Handlers/RegisterUserHandler.cs`, `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryUserRepository.cs`, and the future EF Core user mapping.
- Validation: execute concurrent registrations with the same email and verify exactly one succeeds.

### FIX NOW 10. Resolve worker ownership and deployment boundaries — **PENDING**
- Problem: backtests are processed by `BacktestExecutionWorker` hosted inside the API, while the separate `PortfolioAnalytics.Worker` project only runs a timer and does not consume jobs. This creates an architectural mismatch and can lead to deploying a worker that does no useful work.
- Objective: make the asynchronous execution topology explicit and operationally correct.
- Solution: keep the hosted worker inside the API for the MVP and document the standalone worker as future work. Do not introduce a durable queue until multiple API instances or restart recovery are real requirements.
- Recommendation: remove or archive the unused standalone worker project if it continues to provide no MVP value; revisit a separate worker only together with durable shared storage.
- Where: `src/PortfolioAnalytics.Api/Backtesting/BacktestExecutionWorker.cs`, `src/PortfolioAnalytics.Worker/Worker.cs`, both project startup files, and `ARCHITECTURE.md`.
- Validation: document the selected deployment mode and verify that queued/running jobs are explicitly considered lost on process restart.

## SIMPLIFY - code quality and scope control

These tasks come from a code review focused on keeping the MVP small, readable, and scalable without adding abstractions that do not solve an immediate problem.

### SIMPLIFY 1. Replace message-based exception mapping with typed application errors — **PENDING**
- Problem: `ApiExceptionMiddleware` infers HTTP status codes by searching exception messages. Renaming a message, changing its language, or reusing an exception can change the HTTP contract accidentally.
- Simple solution: introduce only the few typed errors currently needed (`Validation`, `Conflict`, `NotFound`, and authentication failure) and map them directly in the middleware. Do not add a general result framework or a generic error hierarchy.
- Recommendation: keep domain exceptions independent of HTTP and make the API layer responsible for translating application errors into Problem Details.
- Validation: add one test per error type and confirm that messages can change without changing the status code.

### SIMPLIFY 2. Keep one source of truth for backtest ownership — **PENDING**
- Problem: the backtest owner is carried both by `BacktestWorkItem.UserId` and by `RunBacktestCommand.UserId`, creating duplicated state that can diverge.
- Simple solution: keep `UserId` in the command and let the work item contain only the run identifier and command.
- Recommendation: avoid compatibility constructors that preserve obsolete shapes unless an active caller needs them.
- Validation: compile all callers and add a test proving the queued command carries the authenticated owner.

### SIMPLIFY 3. Align async naming with actual behavior — **PENDING**
- Problem: some controller methods end in `Async` but return an immediate `ActionResult`, while in-memory repositories return completed tasks without performing I/O.
- Simple solution: rename synchronous controller methods to `GetRecent`/`GetById`; keep repository async interfaces only as a deliberate compatibility boundary for the future database implementation.
- Recommendation: do not add `async`/`await` merely to satisfy naming conventions.
- Validation: verify route behavior is unchanged and no caller depends on the misleading method names.

### SIMPLIFY 4. Make queue configuration communicate its real policy — **PENDING**
- Problem: the bounded channel is configured with `FullMode.Wait` but uses `TryWrite` and rejects immediately when full.
- Simple solution: use a configuration that clearly represents immediate rejection, or centralize the policy in one small queue abstraction without adding a messaging framework.
- Recommendation: preserve `503 Service Unavailable` for overload and document that the MVP queue is process-local.
- Validation: test normal enqueue, full queue, cancellation, and worker shutdown.

### SIMPLIFY 5. Add explicit request limits before optimizing allocations — **PENDING**
- Problem: market-data batches and backtest ranges have no explicit size limits, so memory and CPU usage are controlled only indirectly.
- Simple solution: add a small strongly typed options object with maximum points, symbol/source lengths, and date range; validate at the API boundary.
- Recommendation: do not optimize collection allocations until safe operational limits exist.
- Validation: test values at, below, and above each limit and preserve the existing Problem Details contract.

### SIMPLIFY 6. Prefer a direct PostgreSQL migration over more in-memory concurrency layers — **PENDING**
- Problem: locks, concurrent dictionaries, snapshots, and compare-and-swap logic make the temporary store increasingly complex.
- Simple solution: keep the current protections stable, but make PostgreSQL + EF Core the next persistence milestone instead of adding more custom in-memory behavior.
- Recommendation: use database transactions and unique indexes for durable invariants; avoid introducing Unit of Work, generic repositories, or CQRS frameworks without a concrete use case.
- Validation: define the minimum schema and migrate one repository at a time with focused integration tests.

### SIMPLIFY 7. Keep testing proportional to the MVP — **PENDING**
- Problem: the backlog points directly to Testcontainers, property-based tests, golden files, and full end-to-end infrastructure, which may delay useful product progress.
- Simple solution: start with focused xUnit domain/use-case tests and a small PostgreSQL integration smoke test using the existing Docker Compose database.
- Recommendation: add Testcontainers, property-based tests, or full end-to-end suites only after a concrete regression or deployment need appears.
- Validation: cover critical authorization, persistence, financial calculation, and HTTP contract paths without creating a new test framework or test platform.

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

### 3. Configure PostgreSQL and the local environment with Docker — **DONE**
- Objective: enable a real database for development and integration testing.
- Owner: backend / DevOps.
- How: define `docker-compose.yml` with PostgreSQL and minimal environment variables.
- Where: project root and `docker/`
- Impact: local development, tests, and deployment.
- Status: completed. Added `docker-compose.yml`, `.env.example`, persistent storage, healthcheck, and README instructions. Runtime startup remains to be verified once Docker Desktop's Linux engine is running.
- Future improvements: add only the next tool justified by a concrete workflow; Redis, pgAdmin, observability, and seed scripts are not MVP requirements.

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
- Future improvements: add one additional strategy only after the first user workflow needs it; defer parameter optimization and multi-asset backtests.

### 9. Run backtests in the background — **DONE**
- Objective: prevent the API from blocking when a heavy calculation is executed.
- Owner: backend + worker.
- How: an endpoint returns 202 Accepted with a job ID, and a worker processes the calculation.
- Where: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Worker/`, `src/PortfolioAnalytics.Infrastructure/BackgroundJobs/`
- Impact: user experience and API scalability.
- Status: completed for the MVP. Backtest requests now return `202 Accepted`, are processed by an in-memory background service, and expose queued, running, completed, or failed status through the existing retrieval endpoint.
- Future improvements: durable job storage or retries only when restart recovery or production workload requires them; defer event-driven processing.

### 10. Save backtest results and key metrics — **DONE**
- Objective: persist results for later comparison.
- Owner: infrastructure + application.
- How: store run summary, strategy, parameters, and calculated metrics.
- Where: `src/PortfolioAnalytics.Domain/Entities/`, `src/PortfolioAnalytics.Infrastructure/Repositories/`
- Impact: the usefulness of the product because it allows strategy comparison over time.
- Status: completed for the MVP. The in-memory execution store keeps each run and updates its status and calculated metrics after background processing. Durable PostgreSQL persistence remains a post-MVP infrastructure step.
- Future improvements: save equity curve series, trade logs, and date-based snapshots.

### 11. Expose MVP REST endpoints — **DONE**
- Objective: make features ready for a frontend or external consumer.
- Owner: API.
- How: build endpoints for login, portfolio, market data, and backtest status/results.
- Where: `src/PortfolioAnalytics.Api/Controllers/`
- Impact: the full usage experience.
- Future improvements: API versioning, pagination, advanced filters, and stable contracts.

## P1 - Product value and feature improvement

### 12. Show performance metrics in a dashboard — **NEXT IN LINE**
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
- How: start with a small smoke-test suite against the PostgreSQL instance already defined in Docker Compose. Introduce Testcontainers only if CI isolation becomes necessary.
- Where: `tests/PortfolioAnalytics.IntegrationTests/` or the existing test project until the suite justifies a split.
- Impact: confidence in real deployment behavior.
- Future improvements: end-to-end API validation and CI migration checks when deployment complexity justifies them.

### 16. Improve deployment and environment readiness — **IN QUEUE**
- Objective: move beyond local-only development.
- Owner: backend + DevOps.
- How: add configuration management, environment variables, Docker Compose, and CI workflows.
- Where: root, `docker/`, and CI config.
- Impact: operational stability and team usability.
- Future improvements: production deployment strategy, health checks, and observability pipeline.

## P2 - Scale and resilience

### 17. Introduce asynchronous processing and queueing — **IN QUEUE**
- Objective: consolidate and document the already implemented MVP background processing instead of creating a second queue architecture.
- Owner: backend + worker.
- How: keep the existing API-hosted bounded channel and worker; migrate to a durable queue only when a concrete multi-instance or restart-recovery requirement exists.
- Where: worker, infrastructure, and background processing modules.
- Impact: performance and scalability.
- Future improvements: add retries or dead-letter handling only after durable jobs are introduced and failure semantics are defined.

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
- Quality refactor completed: portfolio ownership is enforced in the application layer, JWT claim parsing is safe, backtest result updates use immutable snapshots, and template placeholder classes were removed.

## Notes

This project is intended to be a useful financial analysis and strategy platform, not a disconnected architecture demo. The principle is to prioritize pragmatic, Domain-Driven Design (DDD) over theoretical over-engineering.
 confiabilidad del producto.- Mejoras futuras: pruebas end-to-end con cliente y pipeline CI/CD completo.
