# MVP ToDo - PortfolioAnalytics API

This roadmap combines functionality and feasibility. Priority is ordered by value to the user and by the ability to deliver something useful without overbuilding the foundation.

## Current status (September 2026)

The project foundation and the MVP happy path are functional across several key blocks:

- JWT authentication and endpoint protection,
- user registration and login,
- portfolio and position management with domain-based rules,
- MVP market data support with symbol and date-range queries,
- asynchronous backtest submission and result retrieval,
- in-memory repositories as a validation layer for the workflow,
- deterministic sample data and documented local startup.

The valid local flow works:

> register user -> login -> create portfolio -> add positions -> fetch market data -> submit backtest -> retrieve result

However, the current execution and state models are not ready for durable persistence, restart recovery, or horizontal scaling:

- the live backtest flow does not yet enforce `BacktestRun` as the single source of truth;
- mutable string statuses are used alongside the domain `BacktestStatus` state machine;
- calculated metrics are not reliably associated with the original run;
- the in-memory queue has bounded capacity but incomplete shutdown and cancellation semantics;
- the execution result store is an unbounded singleton `ConcurrentDictionary`;
- financial calculation logic still lives in Application instead of Domain;
- current in-memory concurrency tests do not model independent aggregate instances or database-level conflicts.

The immediate objective is therefore not to add database infrastructure or more product features. First, the core state model, domain boundaries, cancellation behavior, and operational limits must be hardened.

## MVP completion checklist before calling the project "useful and working"

This is the exact sequence we should close before declaring the MVP ready for a meaningful demo or public-facing use.

### 1. Validate the end-to-end user journey — **DONE**

- Objective: confirm that the main product flow works without manual corrections.
- Sequence: register user -> login -> create portfolio -> add positions -> fetch market data -> run backtest -> retrieve result.
- Success condition: a user can complete the entire flow in a real run without patching the application or changing the contract ad hoc.
- Status: completed for the happy path. The core flow was exercised successfully in a real local run and the MVP flow is operational for valid requests.
- Hardening note: the happy path does not prove safe shutdown, durable recovery, bounded retention, or concurrency correctness. These are tracked in P0.
- Future improvements: add richer UX flows, dashboards, and additional portfolio operations.

### 2. Stabilize the API contract — **DONE**

- Objective: keep the request and response models clear, consistent, and documented.
- How: review route naming, DTOs, status codes, and payload examples before moving into UI work.
- Success condition: Swagger reflects the actual behavior and the response shape is predictable for clients.
- Status: completed. The API contract was reviewed endpoint by endpoint and validated through the Postman happy and negative flows.
- Hardening note: the backtest status contract must be aligned with the domain state machine before durable persistence.
- Future improvements: versioning, pagination, filtering, and stronger request validation.

### 3. Tighten validation and error handling — **DONE**

- Objective: make invalid scenarios clear and explainable.
- Required checks: invalid dates, empty symbol ranges, duplicate portfolio logic, invalid capital values, and unauthorized requests.
- Success condition: a client gets accurate errors without ambiguous backend exceptions.
- Hardening note: validation must also cover workload size, payload limits, execution time, queue overload, and cancellation.
- Future improvements: structured error envelopes, standardized API error codes, and logging/traceability.

### 3.1. Confirm that HTTP error responses are mapped correctly — **DONE**

- Objective: ensure that all business-rule failures and invalid client inputs result in the correct HTTP status code and a predictable payload.
- Required checks: duplicate user registration should return a conflict/error response instead of an unhandled exception, invalid input should return 400/422, unauthorized access should return 401, and not-found cases should return 404.
- Success condition: the API responds consistently and clearly for every invalid flow a client is expected to hit.
- Status: completed for the web API layer through a centralized exception middleware that translates exceptions into consistent problem-details responses without leaking internal implementation details.
- Hardening note: overload, cancellation, shutdown, and execution-state failures must preserve the same explicit HTTP contract.
- Future improvements: refine the exact payload contract and document the standard error envelope consistently for all endpoints.

### 3.2. Align Postman negative tests with the real API contract — **DONE**

- Objective: prevent false failures caused by expecting success from flows that are intentionally invalid.
- Required checks: duplicate-email registration must assert a non-200 error, requests without a valid JWT must assert 401, missing or invalid route parameters must assert 400/404, and payloads that do not match the API contract must assert validation failures instead of expecting a successful response.
- Success condition: the tests validate the correct business behavior and the correct HTTP semantics, instead of asserting success for scenarios that are supposed to fail.
- Status: completed. The negative Postman cases were finalized and validated against the expected statuses.
- Hardening note: add negative cases for queue saturation, payload limits, execution cancellation, and rejected work during shutdown.
- Future improvements: create a dedicated negative-test collection with explicit expected statuses for duplicate registration, invalid ids, unauthorized access, invalid form bodies, and empty time ranges.

### 4. Document local configuration and startup procedure — **DONE**

- Objective: ensure the project can be run by another engineer or reviewer without hidden setup steps.
- Required items: environment variables, JWT config, startup commands, sample data behavior, and local API calls.
- Success condition: a contributor can run the app locally using the project instructions and reproduce the main flow.
- Status: completed. README documents startup, JWT configuration, Swagger, health checks, sample data, and the reproducible API flow.
- Hardening note: document that the current in-memory execution state is process-local and that P0 must be completed before treating the flow as durable.
- Future improvements: Docker Compose automation, environment profiles, and deployment-ready configuration.

### 5. Decide the persistence strategy explicitly — **DONE**

- Objective: clarify whether the current in-memory store is temporary or part of the real MVP contract.
- Required decision: keep in-memory repositories only as an intentional MVP stage, and plan PostgreSQL + EF Core as the next real persistence milestone.
- Success condition: the architecture document and the README clearly explain the reasoning.
- Status: completed. The project documents in-memory repositories as a temporary MVP implementation and PostgreSQL + EF Core as the next persistence stage.
- Hardening note: PostgreSQL and EF Core are blocked until P0 resolves the state machine, domain boundaries, concurrency semantics, and repository contracts.
- Future improvements: migrations, repository implementation, transactions, and durable data storage.

### 6. Confirm the MVP is demo-ready with deterministic sample data — **DONE**

- Objective: avoid depending on external providers too early.
- How: keep a minimal, repeatable dataset and a predictable run that can be re-used for demos and testing.
- Success condition: the project can be demonstrated without brittle or flaky external dependencies.
- Hardening note: deterministic data must produce metrics associated with the correct `BacktestRun` and remain reproducible after the Domain calculation move.
- Future improvements: real market-data ingestion, provider integrations, and normalization pipelines.

### 7. Add basic operational checks — **DONE**

- Objective: make the app easier to run, diagnose, and trust.
- Required items: a health or status endpoint, clear logs for failed requests, and a minimal note in the README about the demo flow.
- Success condition: a developer can tell whether the app is healthy and whether a failure is expected or a real bug.
- Status: completed. `/health` provides a basic liveness check, the middleware logs unhandled failures, and README documents the demo flow.
- Hardening note: health checks must not imply that queued work is durable while execution remains process-local.
- Future improvements: observability, dashboards, tracing, and performance metrics.

## FIX NOW - API security, correctness, and resilience

These fixes must be completed before exposing the API outside a trusted local development environment. They address issues found during the .NET API review and take priority over new product features.

### FIX NOW 1. Isolate backtests by authenticated user — **DONE**

- Objective: prevent one authenticated user from listing or reading backtest runs created by another user.
- What to do: associate every `BacktestRunResponse` and queued `BacktestWorkItem` with the `UserId` extracted from the JWT. Filter recent runs by that owner and return not found when a requested run belongs to another user.
- How to do it: add `UserId` to the backtest request/response flow, pass it from `BacktestsController` into the command and queue item, and update `BacktestExecutionStore.GetRecent` and `GetById` to require the current user identifier. Preserve the existing portfolio ownership pattern so cross-user resources are not enumerable.
- Where: `src/PortfolioAnalytics.Api/Controllers/BacktestsController.cs`, `src/PortfolioAnalytics.Application/Commands/`, `src/PortfolioAnalytics.Application/DTOs/`, `src/PortfolioAnalytics.Application/Services/BacktestExecutionStore.cs`, and `src/PortfolioAnalytics.Application/Services/BacktestExecutionQueue.cs`.
- Status: completed. The controller extracts `ClaimTypes.NameIdentifier`, stores it on the response, command, and queue item, and requires the same owner for recent and individual run reads. Requests with a missing or invalid identity are unauthorized, while another user's run is indistinguishable from a missing run (`404 Not Found`).
- Hardening note: ownership must remain part of the durable run identity and every future database query must stay owner-scoped.
- Validation: focused unit tests prove owner filtering for queued, running, completed, and failed backtests and verify that queue items carry the authenticated owner.

### FIX NOW 2. Remove the known fallback JWT key outside Development — **DONE**

- Objective: ensure a deployment cannot run with a publicly known signing secret.
- What to do: keep a development-only local fallback if desired, but fail application startup in non-Development environments when `Jwt:Key` is missing or too weak. Require a sufficiently long secret supplied through configuration or an environment variable.
- How to do it: validate JWT settings during service registration, use `IHostEnvironment` to distinguish Development from other environments, and configure production values through `Jwt__Key`, `Jwt__Issuer`, and `Jwt__Audience`. Do not log the secret.
- Where: `src/PortfolioAnalytics.Api/Program.cs`, `src/PortfolioAnalytics.Infrastructure/Identity/JwtSettings.cs`, `src/PortfolioAnalytics.Infrastructure/Identity/JwtTokenService.cs`, `tests/PortfolioAnalytics.UnitTests/JwtSettingsTests.cs`, `README.md`, and `.env.example`.
- Impact: security and deployment correctness.
- Status: completed. Startup loads one validated `JwtSettings` instance. Development may use the local fallback, while every other environment fails before service registration when `Jwt:Key` is missing or shorter than 32 characters. Authentication and token generation use the same validated settings, and no secret is logged.
- Validation: focused unit tests cover the Development fallback, missing/weak non-Development keys, and valid configured settings; the API build also verifies composition-root wiring.

### FIX NOW 3. Make singleton in-memory repositories thread-safe — **DONE**

- Objective: prevent data races and collection corruption when concurrent HTTP requests access the singleton repositories.
- What to do: replace mutable `Dictionary` instances with `ConcurrentDictionary`, or protect compound reads and writes with a synchronization strategy. Keep returned collections as snapshots so callers cannot enumerate a collection while it is being modified.
- How to do it: apply the same approach consistently to users, portfolios, and market data; review duplicate-check-then-insert flows because they must be atomic or explicitly documented as MVP limitations.
- Where: `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryUserRepository.cs`, `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryPortfolioRepository.cs`, and `src/PortfolioAnalytics.Infrastructure/Repositories/InMemoryMarketDataRepository.cs`.
- Impact: process-local correctness under concurrent requests.
- Status: completed. Singleton repositories now use concurrent dictionaries, lock compound user writes and portfolio repository operations, and return materialized query snapshots. Portfolio position duplicate-check/add is synchronized inside the aggregate so concurrent read-modify-write requests do not corrupt the position collection. Uniqueness is guaranteed only within this process; the PostgreSQL unique index remains the durable invariant.
- Hardening note: this does not model independent aggregate instances, optimistic concurrency, transactions, or database conflict behavior. Those must be covered in P1 integration work after P0.
- Validation: focused xUnit tests cover concurrent user registration attempts, portfolio position updates, and market-data upserts/snapshots; the existing unit suite remains green.

### FIX NOW 4. Bound the backtest queue and honor request cancellation — **DONE, P0 HARDENING REQUIRED**

- Objective: prevent unlimited queued work from exhausting memory and avoid accepting work after the client request has been cancelled.
- What to do: configure a bounded channel with an explicit capacity, return a clear overload response when the queue is full, and pass the controller cancellation token to enqueue instead of `CancellationToken.None`.
- How to do it: use a bounded `Channel<BacktestWorkItem>` with a defined full-mode policy, handle the failed write in the controller, and preserve cancellation behavior in the background worker. Add rate limiting if the endpoint will be publicly reachable.
- Where: `src/PortfolioAnalytics.Application/Services/BacktestExecutionQueue.cs`, `src/PortfolioAnalytics.Api/Controllers/BacktestsController.cs`, and `src/PortfolioAnalytics.Api/Backtesting/BacktestExecutionWorker.cs`.
- Impact: memory protection and admission control.
- Status: completed for bounded capacity and request-token propagation. The in-process queue uses a bounded channel with an explicit capacity of 100 and immediate full detection. The controller passes its request token to enqueue, removes the provisional run when enqueue is cancelled or overloaded, returns `503 Service Unavailable` when full, and leaves the worker's shutdown cancellation behavior unchanged.
- Hardening note: bounded capacity alone is insufficient. P0 must add explicit shutdown reconciliation, deep cancellation propagation, execution timeouts, workload limits, and a durable state model before production or multi-instance deployment.
- Validation: focused xUnit tests cover queue saturation, cancelled requests, accepted queued responses, and worker transition to `Completed`. Queue state remains process-local and is lost on restart; the P0 remediation section defines the required correction.

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

### FIX NOW 7. Add bounded HTTP payload and workload limits — **PENDING, MOVE TO P0**

- Problem: market-data ingestion accepts an unbounded enumerable, and backtest requests do not limit the date range or the amount of pending work. An authenticated client could consume excessive memory or CPU even without exploiting a lower-level vulnerability.
- Objective: keep request processing and background workload within explicit MVP operating limits and prevent CPU or memory denial of service.
- Solution: define maximum market-data points per request, maximum symbol/source lengths, maximum backtest date range, maximum points per simulation, maximum execution time, and maximum pending jobs per user and globally. Reject invalid sizes with the standard `400 Bad Request` problem-details response and use overload protection for the queue.
- Recommendation: configure limits through strongly typed options instead of hardcoding them in controllers, document the values in Swagger/README, and add rate limiting before exposing the API publicly.
- Where: `src/PortfolioAnalytics.Api/Controllers/MarketDataController.cs`, `src/PortfolioAnalytics.Api/Controllers/BacktestsController.cs`, request DTOs, queue configuration, and API configuration.
- Validation: test requests at the boundary, just above the boundary, queue saturation behavior, and a backtest that exceeds the execution timeout.

### FIX NOW 8. Enforce invariant ISO date parsing in market-data queries — **PENDING**

- Objective: make the HTTP date contract deterministic across environments.
- Problem: the API error message promises `yyyy-MM-dd`, but `DateOnly.TryParse` uses culture-sensitive parsing. The same request can therefore be interpreted differently depending on the server culture.
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
- Solution: keep the hosted worker inside the API for the MVP and document the standalone worker as future work. Do not introduce a durable queue until P0 has defined the run state machine, cancellation semantics, ownership boundary, and recovery behavior.
- Recommendation: remove or archive the unused standalone worker project if it continues to provide no MVP value; revisit a separate worker only together with durable shared storage.
- Where: `src/PortfolioAnalytics.Api/Backtesting/BacktestExecutionWorker.cs`, `src/PortfolioAnalytics.Worker/Worker.cs`, both project startup files, and `ARCHITECTURE.md`.
- Validation: document the selected deployment mode and verify that queued/running jobs are explicitly considered lost on process restart until P1 durable persistence is implemented.

## P0 - Core State, Domain Purity & Remediation (Pre-Database)

These items are immediate priority. PostgreSQL, EF Core, Dapper, horizontal scaling, and durable job infrastructure are blocked until this section is complete. The system must not allow infrastructure concerns to define or preserve an invalid state machine.

### P0.1. Move financial calculation logic into Domain — **PENDING**

- Objective: keep core financial rules independent from HTTP, repositories, and infrastructure.
- Scope: move buy-and-hold execution and the calculation of annualized return, Sharpe ratio, drawdown, volatility, and related metrics out of `PortfolioAnalytics.Application`.
- Design: introduce the smallest Domain-level calculation or strategy component justified by the current MVP. It must operate on normalized domain data and remain deterministic.
- Where: move logic from `src/PortfolioAnalytics.Application/Services/BacktestService.cs` into `src/PortfolioAnalytics.Domain/` and keep Application responsible for orchestration only.
- Validation: add focused xUnit tests for valid calculations, edge cases, deterministic results, and invalid or insufficient time series.
- Acceptance criteria: Application handlers retrieve data and invoke Domain behavior; they do not contain financial formulas or strategy rules.

### P0.2. Enforce `BacktestRun` as the single source of truth — **PENDING**

- Objective: replace mutable string state with the domain state machine.
- Required states: `Queued`, `Running`, `Succeeded`, `Failed`, and `Cancelled`.
- Scope: every transition must go through guarded `BacktestRun` domain methods. Remove direct worker/controller mutation of string statuses such as `"Completed"`.
- Design: define valid transitions, failure information, cancellation behavior, and whether a run can be retried. Keep API DTO status values mapped from the domain enum rather than defining a second state model.
- Where: `src/PortfolioAnalytics.Domain/Entities/BacktestRun.cs`, `BacktestStatus`, application handlers, execution worker, DTO mapping, and tests.
- Validation: test every valid transition and reject invalid transitions, including shutdown cancellation and execution failure.
- Acceptance criteria: one authoritative status model exists from submission through result retrieval and future persistence.

### P0.3. Associate `PerformanceMetrics` with the original run — **PENDING**

- Objective: ensure every result belongs to the exact `BacktestRun` that produced it.
- Scope: remove the generated or unrelated metrics identifier behavior and pass the original run ID through the command, worker, Domain calculation, and result update.
- Design: `PerformanceMetrics.BacktestRunId` must equal the submitted `BacktestRun.Id`; no calculation service may invent a replacement run identifier.
- Where: `BacktestService`, `RunBacktestHandler`, `BacktestRun`, `PerformanceMetrics`, execution store, and related DTOs/tests.
- Validation: submit a run, complete it, and assert that the returned metrics reference the same run ID and owner.
- Acceptance criteria: a result can be joined unambiguously to its input parameters, state, owner, and execution history.

### P0.4. Define graceful shutdown and state reconciliation — **PENDING**

- Objective: avoid leaving queued or running jobs in false states when the host stops.
- Scope: stop accepting new work, define whether pending jobs are drained or cancelled, cancel active work, and transition every affected run through the domain state machine.
- Design: use a linked cancellation token for host shutdown and per-execution timeout; ensure queued items not consumed by the worker are explicitly reconciled rather than silently abandoned.
- Where: `BacktestExecutionQueue`, `BacktestExecutionWorker`, `BacktestExecutionStore`, `BacktestRun`, and API startup/shutdown registration.
- Validation: test shutdown with queued jobs, an active job, a completed job, and a failed job. Confirm no run remains indefinitely in `Queued` or `Running`.
- Acceptance criteria: shutdown behavior is documented and observable, even though the current implementation remains process-local.

### P0.5. Propagate `CancellationToken` through the complete execution path — **PENDING**

- Objective: make cancellation responsive during I/O and CPU-heavy simulation work.
- Scope: pass cancellation through repository calls, data normalization, sorting, strategy execution, metric loops, result persistence, and worker shutdown.
- Design: check cancellation at bounded intervals inside expensive loops and propagate the token rather than catching and hiding `OperationCanceledException`.
- Where: `RunBacktestHandler`, Domain backtest calculation boundary, market-data repositories, worker, and tests.
- Validation: cancel during data retrieval and during calculation; verify that the run becomes `Cancelled` and is not reported as successful.
- Acceptance criteria: cancellation is explicit, testable, and not reduced to a single pre-enqueue check.

### P0.6. Add workload and payload limits — **PENDING**

- Objective: prevent authenticated clients from consuming unbounded CPU, memory, or queue capacity.
- Scope: limit market-data payload size, symbol/source lengths, backtest date range, maximum input points, execution duration, global queue size, and pending jobs per user.
- Design: use strongly typed, startup-validated options. Reject invalid requests at the API boundary and return a clear overload response when admission fails.
- Where: API request validation, options configuration, queue, market-data ingestion, backtest command, and documentation.
- Validation: test below-limit, boundary, and above-limit requests; test a large but valid request with an execution timeout.
- Acceptance criteria: resource consumption is constrained by explicit policy rather than accidental in-memory behavior.

### P0.7. Replace unbounded execution state with bounded retention — **PENDING**

- Objective: prevent `BacktestExecutionStore` from becoming an unbounded memory leak before durable storage exists.
- Scope: replace the singleton `ConcurrentDictionary` retention behavior with an explicit bounded strategy, such as a maximum item count plus TTL for terminal runs.
- Design: preserve owner-scoped reads, make eviction thread-safe, and document whether expired results return `404`. Do not introduce a general cache framework without a concrete need.
- Where: `src/PortfolioAnalytics.Application/Services/BacktestExecutionStore.cs`, configuration options, result queries, and tests.
- Validation: insert more than the configured capacity, advance past retention time where testable, and confirm terminal results are evicted without affecting active runs.
- Acceptance criteria: memory usage is bounded by configuration and active runs are never silently discarded.

### P0.8. Remove duplicated ownership and execution state representations — **PENDING**

- Objective: keep one authoritative representation for run identity, owner, command parameters, and status.
- Scope: remove compatibility fields or constructors that allow inconsistent `UserId` values between the command and `BacktestWorkItem`. Ensure worker updates are owner-aware.
- Design: use the run identifier as the primary execution reference and keep `UserId` persisted and validated at every read/update boundary.
- Where: command, work item, queue, worker, execution store, and tests.
- Validation: attempt inconsistent owner inputs and confirm they are rejected before enqueue or update.
- Acceptance criteria: no internal producer can create a work item whose owner differs from the persisted run.

### P0.9. Make the queue policy explicit — **PENDING**

- Objective: ensure configuration communicates the actual overload behavior.
- Scope: align bounded-channel configuration with immediate rejection through `TryWrite`, or use a small direct abstraction that makes the policy explicit.
- Design: preserve `503 Service Unavailable` for a full queue and avoid blocking HTTP request threads. Define stop-accepting behavior during shutdown.
- Where: `BacktestExecutionQueue`, queue options, controller, and tests.
- Validation: test normal enqueue, full queue, cancellation during enqueue, shutdown admission rejection, and worker completion.
- Acceptance criteria: queue capacity, overload, cancellation, and shutdown semantics are explicit and documented.

### P0.10. Replace duplicated controller orchestration with an application command — **PENDING**

- Objective: keep HTTP transport concerns out of the backtest use case.
- Scope: move run creation, validation coordination, initial state creation, enqueueing, and admission failure handling from `BacktestsController` into an application command handler.
- Design: the controller maps HTTP input and output only. Application owns the use-case flow; Domain owns financial rules and state transitions.
- Where: `BacktestsController`, `RunBacktestCommand`, a submit-backtest handler, DTO mapping, and tests.
- Validation: preserve the existing HTTP contract while testing the command independently from ASP.NET.
- Acceptance criteria: no controller directly decides domain status strings or coordinates queue/store internals.

### P0.11. Replace false-positive concurrency tests — **PENDING**

- Objective: test the concurrency behavior that will exist with real persistence.
- Scope: load independent copies of an aggregate, apply concurrent modifications, and verify conflict detection or explicit last-write behavior.
- Design: retain process-local thread-safety tests, but clearly separate them from aggregate consistency tests. Do not treat a shared mutable in-memory instance as equivalent to EF Core or Dapper behavior.
- Where: `tests/PortfolioAnalytics.UnitTests/` and future integration test coverage.
- Validation: add tests for stale portfolio updates, concurrent position additions, queue producers, cancellation, shutdown, and result-store retention.
- Acceptance criteria: tests expose lost updates and invalid state transitions before database migration.

## SIMPLIFY - code quality and scope control

These tasks come from a code review focused on keeping the MVP small, readable, and scalable without adding abstractions that do not solve an immediate problem.

### SIMPLIFY 1. Replace message-based exception mapping with typed application errors — **PENDING**

- Problem: `ApiExceptionMiddleware` infers HTTP status codes by searching exception messages. Renaming a message, changing its language, or reusing an exception can change the HTTP contract accidentally.
- Simple solution: introduce only the few typed errors currently needed (`Validation`, `Conflict`, `NotFound`, and authentication failure) and map them directly in the middleware. Do not add a general result framework or a generic error hierarchy.
- Recommendation: keep domain exceptions independent of HTTP and make the API layer responsible for translating application errors into Problem Details.
- Validation: add one test per error type and confirm that messages can change without changing the status code.

### SIMPLIFY 2. Keep one source of truth for backtest ownership — **PENDING**

- Problem: the backtest owner is carried both by `BacktestWorkItem.UserId` and by `RunBacktestCommand.UserId`.
- Simple solution: keep `UserId` authoritative on the persisted/domain run and let the work item contain only the run identifier plus the minimum execution reference required by the worker.
- Recommendation: avoid compatibility constructors that preserve obsolete shapes unless an active caller needs them. Coordinate this cleanup with P0 state management.
- Validation: compile all callers and add a test proving that the queued command carries the authenticated owner.

### SIMPLIFY 3. Align async naming with actual behavior — **PENDING**

- Problem: some controller methods end in `Async` but return an immediate `ActionResult`, while in-memory repositories return completed tasks without performing I/O.
- Simple solution: rename synchronous controller methods to `GetRecent`/`GetById`; keep repository async interfaces only as a deliberate compatibility boundary for the future database implementation.
- Recommendation: do not add `async`/`await` merely to satisfy naming conventions.
- Validation: verify route behavior is unchanged and no caller depends on the misleading method names.

### SIMPLIFY 4. Make queue configuration communicate its real policy — **PENDING**

- Problem: the bounded channel is configured with `FullMode.Wait` but uses `TryWrite` and rejects immediately when full.
- Simple solution: use a configuration that clearly represents immediate rejection, or centralize the policy in one small queue abstraction without adding a messaging framework.
- Recommendation: preserve `503 Service Unavailable` for overload and document that the MVP queue is process-local. Coordinate with P0 shutdown and admission semantics.
- Validation: test normal enqueue, full queue, cancellation, shutdown, and worker reconciliation.

### SIMPLIFY 5. Add explicit request limits before optimizing allocations — **PENDING**

- Problem: market-data batches and backtest ranges have no explicit size limits, so memory and CPU usage are controlled only indirectly.
- Simple solution: add a small strongly typed options object with maximum points, symbol/source lengths, date range, and execution duration; validate at the API boundary.
- Recommendation: do not optimize collection allocations until safe operational limits exist.
- Validation: test values at, below, and above each limit and preserve the existing Problem Details contract.

### SIMPLIFY 6. Prefer a direct PostgreSQL migration over more in-memory concurrency layers — **PENDING, BLOCKED BY P0**

- Problem: locks, concurrent dictionaries, snapshots, and compare-and-swap logic make the temporary store increasingly complex.
- Simple solution: complete P0 first, then make PostgreSQL + EF Core the next persistence milestone instead of adding more custom in-memory behavior.
- Recommendation: use database transactions and unique indexes for durable invariants; avoid introducing Unit of Work, generic repositories, or CQRS frameworks without a concrete use case.
- Validation: begin schema and repository migration only after P0 defines valid states, ownership, concurrency behavior, and result retention semantics.

### SIMPLIFY 7. Keep testing proportional to the MVP — **PENDING**

- Problem: the backlog points directly to Testcontainers, property-based tests, golden files, and full end-to-end infrastructure, which may delay useful product progress.
- Simple solution: start with focused xUnit domain/use-case tests, P0 execution tests, and a small PostgreSQL integration smoke test after the P1 database gate is open.
- Recommendation: add Testcontainers, property-based tests, or full end-to-end suites only after a concrete regression or deployment need appears.
- Validation: cover critical authorization, persistence, financial calculation, concurrency, cancellation, and HTTP contract paths without creating a new test framework or test platform.

## P1 - Durable persistence and product value

P1 starts only after P0 is complete. PostgreSQL + EF Core is intentionally blocked until the state machine, domain boundaries, execution cancellation, workload limits, and repository semantics are defined. Infrastructure must not dictate core business rules or persist a broken state model.

### P1.1. Implement PostgreSQL + EF Core persistence — **IN QUEUE, BLOCKED BY P0**

- Objective: replace temporary in-memory persistence with durable storage.
- Prerequisite: complete all P0 items and document the resulting state, ownership, cancellation, retention, and concurrency rules.
- Scope: persist users, portfolios, positions, market data, `BacktestRun`, and `PerformanceMetrics`.
- Required invariants:
  - `BacktestRun` is the authoritative state aggregate;
  - `PerformanceMetrics.BacktestRunId` references the original run;
  - owner-scoped reads and updates are mandatory;
  - valid domain transitions are enforced before persistence;
  - optimistic concurrency is defined for aggregate updates;
  - unique constraints exist for normalized user email and market-data deduplication;
  - decimal precision, UTC dates, lengths, indexes, and relationships are explicit.
- Recommendation: map EF Core to the existing Domain model without moving business rules into `DbContext` or repository implementations. Do not let database constraints replace domain validation.
- Where: `src/PortfolioAnalytics.Infrastructure/`, EF Core mappings, migrations, repository implementations, and integration tests.
- Validation: run focused PostgreSQL smoke tests for migrations, ownership, unique constraints, concurrent updates, state transitions, and result-to-run relationships.

### P1.2. Define explicit repository contracts for durable storage — **IN QUEUE, BLOCKED BY P0**

- Objective: avoid carrying in-memory semantics into EF Core or Dapper by accident.
- Scope: define not-found behavior, update results, concurrency conflicts, transaction boundaries, filtering, ordering, and pagination.
- Recommendation: use explicit read DTOs/projections for large market-data queries where appropriate. Do not require Dapper or EF Core to hydrate complex aggregates implicitly.
- Validation: document the contract and implement one repository at a time with focused integration tests.

### P1.3. Implement integration tests with real PostgreSQL — **IN QUEUE, BLOCKED BY P0**

- Objective: ensure the database, repositories, and API work together correctly.
- How: start with a small smoke-test suite against the PostgreSQL instance already defined in Docker Compose. Introduce Testcontainers only if CI isolation becomes necessary.
- Where: `tests/PortfolioAnalytics.IntegrationTests/` or the existing test project until the suite justifies a split.
- Impact: confidence in real deployment behavior.
- Validation: cover migrations, aggregate concurrency, unique constraints, owner isolation, backtest state transitions, cancellation outcomes, and metrics linkage.

### P1.4. Show performance metrics in a dashboard — **NEXT IN LINE AFTER P0**

- Objective: make results understandable to the user.
- Owner: frontend + API + analytics.
- How: return summary metrics so the client can render graphs.
- Where: `src/PortfolioAnalytics.Api/` and `client/`
- Impact: product adoption and decision-making.
- Future improvements: benchmark comparisons, heatmaps, equity curves, drawdown charts, and CSV/PDF export.

### P1.5. Compare strategies against each other — **IN QUEUE**

- Objective: help users evaluate which strategy performs best on the same portfolio.
- Owner: backend + frontend.
- How: save several runs and compare their metrics only after run identity and result persistence are durable.
- Where: `src/PortfolioAnalytics.Application/Queries/` and `src/PortfolioAnalytics.Api/`
- Impact: the analytical usefulness of the product.
- Future improvements: automated parameter optimization and rankings by weighted metrics.

### P1.6. Improve deployment and environment readiness — **IN QUEUE**

- Objective: move beyond local-only development.
- Owner: backend + DevOps.
- How: add configuration management, environment variables, Docker Compose, and CI workflows after durable persistence and execution recovery are defined.
- Where: root, `docker/`, and CI config.
- Impact: operational stability and team usability.
- Future improvements: production deployment strategy, health checks, and observability pipeline.

## P2 - Scale and resilience

### P2.1. Introduce durable asynchronous processing and queueing — **IN QUEUE**

- Objective: support restart recovery and multiple API instances without duplicating or losing work.
- Prerequisite: P0 state semantics and P1 durable run persistence must be complete.
- How: introduce a durable job record and an atomic claim/lease protocol, or an external durable messaging service only if a concrete operational need justifies it.
- Where: worker, infrastructure, durable job repository, and background processing modules.
- Impact: horizontal scalability, recovery, and reliable execution.
- Future improvements: retries, dead-letter handling, and externalized worker deployment only after failure semantics are defined.

### P2.2. Add real market-data integrations — **IN QUEUE**

- Objective: move from local test data to provider-backed market feeds.
- Owner: infrastructure + application.
- How: implement provider adapters and normalization layers for incoming market data while preserving Domain validation and deduplication rules.
- Where: `src/PortfolioAnalytics.Infrastructure/ExternalServices/`
- Impact: product realism and analytic quality.
- Future improvements: multiple providers, backfills, and alerting for failed syncs.

## What comes after the MVP: product, technical, and integration improvements

Once the project is useful, P0 is hardened, and P1 persistence is stable, the remaining work shifts from basic functionality to product expansion and scaling.

### Product improvements — **IN QUEUE**

- portfolio dashboard and historical results;
- charting for drawdown, equity curve, and performance snapshots;
- saved strategies and parameter presets;
- better comparison between strategy runs;
- portfolio rebalancing and watchlists.

### Technical improvements — **IN QUEUE**

- durable background processing and job recovery;
- repository and unit-of-work cleanup only where a concrete transaction boundary requires it;
- integration testing with real database and API validation;
- CI/CD, deployment pipelines, and environment automation;
- observability, tracing, and performance metrics.

### External integrations — **IN QUEUE**

- real market-data providers;
- broker or exchange APIs;
- authentication providers or enterprise identity integration;
- notifications, alerts, and reporting flows.

## Implementation guidance

- Keep the MVP focused on real user value, not theoretical architecture.
- Treat P0 as a prerequisite for durable persistence, not as a scalability rewrite.
- Keep `BacktestRun` and its domain state machine explicit and stable before adding database or queue infrastructure.
- Keep financial logic in Domain; keep Application focused on use-case orchestration.
- Use strongly typed options and startup validation for queue capacity, payload limits, execution timeouts, and rate limits.
- Propagate `CancellationToken` through all asynchronous I/O and expensive calculations.
- Use tests as a safety net for critical financial rules, state transitions, concurrency, cancellation, ownership, and HTTP contracts.
- Keep repository contracts explicit about persistence semantics instead of relying on in-memory behavior.
- Keep documentation aligned with the current state of the project as the code evolves.
- Do not introduce generic repositories, event buses, distributed queues, or additional abstractions without a concrete MVP requirement.
- Quality refactor completed: portfolio ownership is enforced in the application layer, JWT claim parsing is safe, backtest result updates use immutable snapshots, and template placeholder classes were removed.
- The completed MVP items remain valid for the happy path, but they require the P0 concurrency and state hardening described above before they can be considered durable or production-ready.

## Notes

This project is intended to be a useful financial analysis and strategy platform, not a disconnected architecture demo. The principle is to prioritize pragmatic, Domain-Driven Design (DDD) over theoretical over-engineering.

The current in-memory implementation is acceptable as a temporary local MVP. It must not become the source of persistence semantics by accident. The next architectural milestone is to make the Domain state, financial calculations, cancellation behavior, workload limits, and execution retention explicit before PostgreSQL or distributed processing is introduced.# MVP ToDo - PortfolioAnalytics API

