# Architecture overview

This document serves as the live record of the project architecture. It helps to:
- visualize how the solution is assembled,
- understand what each layer is responsible for,
- record design decisions,
- maintain an educational reference as the project evolves.

This file should be updated whenever the main architecture, execution flow, or system structure changes.

## 1. General purpose of the system

The project aims to become a financial analysis and portfolio management API focused on:
- managing users,
- creating and maintaining portfolios,
- synchronizing market data,
- evaluating investment strategies through backtesting,
- calculating key performance and risk metrics,
- exposing functionality to a frontend client or other consumers.

The main MVP goal is to solve a real and useful case:
"a user can manage a portfolio, load market data, and run strategy simulations against their portfolio."

## 2. General architecture

The solution is designed with separation by layers to keep the project clear and allow gradual evolution without mixing responsibilities.

```text
┌──────────────────────────────────────────────────────────────────────┐
│                           Client / Consumers                         │
│  Web app / Mobile app / External API consumer / Swagger UI         │
└───────────────────────────────┬──────────────────────────────────────┘
                                 │ HTTP / REST
                                 ▼
┌──────────────────────────────────────────────────────────────────────┐
│                         PortfolioAnalytics.Api                       │
│ - Controllers                                                       │
│ - DTOs / request contracts                                          │
│ - Middleware                                                        │
│ - Dependency injection                                              │
│ - Swagger / API surface                                              │
└───────────────────────────────┬──────────────────────────────────────┘
                                 │ uses
                                 ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    PortfolioAnalytics.Application                    │
│ - Commands                                                          │
│ - Queries                                                          │
│ - Handlers                                                          │
│ - Validators                                                        │
│ - Services                                                          │
│ - Business flow orchestration                                       │
└───────────────────────────────┬──────────────────────────────────────┘
                                 │ uses
                                 ▼
┌──────────────────────────────────────────────────────────────────────┐
│                     PortfolioAnalytics.Domain                        │
│ - Entities                                                          │
│ - Value Objects                                                     │
│ - Enums                                                            │
│ - Interfaces                                                        │
│ - Business rules                                                    │
│ - Exceptions                                                       │
└───────────────────────────────┬──────────────────────────────────────┘
                                 │ implemented by
                                 ▼
┌──────────────────────────────────────────────────────────────────────┐
│                 PortfolioAnalytics.Infrastructure                    │
│ - EF Core DbContext                                                 │
│ - Repositories                                                      │
│ - Dapper queries                                                    │
│ - HTTP clients                                                      │
│ - JWT / auth implementations                                        │
│ - Background jobs / worker integrations                             │
└───────────────────────────────┬──────────────────────────────────────┘
                                 │ async jobs / integration
                                 ▼
┌──────────────────────────────────────────────────────────────────────┐
│                       PortfolioAnalytics.Worker                     │
│ - Heavy tasks                                                       │
│ - Future externalized backtest execution                            │
│ - Market sync or asynchronous processing                             │
└──────────────────────────────────────────────────────────────────────┘
```

## 3. What each layer does

### 3.1. Domain
This is the purest layer in the project. It does not know about ASP.NET, EF Core, or the API.

Responsibilities:
- define business entities,
- encapsulate core business rules,
- give structure to the financial domain,
- define repository contracts,
- maintain domain exceptions and validations.

Current examples:
- `User`
- `Portfolio`
- `Position`
- `MarketDataPoint`
- `StrategyDefinition`
- `BacktestRun`
- `PerformanceMetrics`

### 3.2. Application
This is the use-case layer.

Responsibilities:
- execute product actions,
- prepare commands and queries,
- orchestrate validation and service calls,
- coordinate business logic that does not belong to a single isolated entity,
- prepare DTOs for the API layer.

Representative examples:
- `CreatePortfolioCommand`
- `AddPositionCommand`
- `RunBacktestCommand`
- `GetPortfolioSummaryQuery`
- `CreatePortfolioHandler`

This layer follows a CQRS-oriented structure, separating commands from queries and keeping handlers focused on a single use case.

### 3.3. Infrastructure
This is the technical implementation layer.

Responsibilities:
- persistence with EF Core,
- concrete repositories,
- access to external market data sources,
- JWT or technical security,
- job processing and heavy tasks,
- any external dependency.

Examples:
- `AppDbContext`
- `PortfolioRepository`
- `MarketDataRepository`
- `YahooFinanceClient`
- `JwtTokenService`

The persistence design follows the Repository Pattern, abstracted for upcoming PostgreSQL/EF Core integration while remaining in-memory for the current MVP.

### 3.4. API
This is the HTTP exposure layer.

Responsibilities:
- receive REST requests,
- map input and output DTOs,
- call the application layer,
- handle transport-level errors,
- expose Swagger and minimal API documentation.

Examples:
- `AuthController`
- `PortfoliosController`
- `MarketDataController`
- `BacktestsController`

### 3.5. Worker
This is the layer for heavy processing and asynchronous tasks.

Responsibilities:
- execute heavy backtests,
- synchronize market data sources,
- keep long-running work outside the request thread,
- avoid blocking the API.

For the current MVP, the backtest background service is hosted by the API so the queue
and in-memory result store remain in one process. The standalone Worker project is kept
for the future stage where jobs move to durable or distributed infrastructure.

## 4. Design principles

### 4.1. Separation of responsibilities
Each layer has a clear purpose and does not mix with another.

### 4.2. Pure domain
The domain is the business reference. It does not need to know whether there is a web API, a database, or HTTP.

### 4.3. Repositories over persistence details
The infrastructure layer implements repositories; the application layer does not depend on how data is stored.

### 4.4. Explicit messages and invariants
Commands, queries, and entities express the business in a visible and verifiable way.

## 5. Current functional flow

### 5.1. Registration and login
- The user sends email, name, and password.
- The application validates input and hashes the password.
- A JWT is issued for the session.

### 5.2. Portfolio management
- The user creates a portfolio through the API.
- The use case validates the name, owner, and operation integrity.
- The application layer coordinates with the repository and infrastructure.

### 5.3. Position management
- The user adds a position with symbol, quantity, and value.
- The `Portfolio` entity validates that there are no duplicate symbols.
- The operation is persisted by the current in-memory repository.

### 5.4. Market data
- A market-price series is synchronized with date and symbol.
- Duplicate records are deduplicated by a functional key.
- The system can query a date window by symbol.

## 6. Current architecture state

The project already has a useful MVP foundation:
- JWT for authentication,
- repository for users,
- repository for portfolios,
- repository for market data,
- token-protected API,
- use cases encapsulated by handlers.

The current persistence layer is in-memory by design and is intentionally set to evolve toward PostgreSQL + EF Core later.

## 7. Relevant design decisions

### Repository pattern
The infrastructure layer defines repository abstractions so the application does not depend on a concrete storage implementation.

### CQRS-oriented application layer
The application organizes logic into commands, queries, and handlers. This keeps reading and writing separate, preserves clarity, and prepares the codebase for growth without mixing responsibilities.

### Backtest ownership boundary
Backtest runs are isolated by the authenticated user's `ClaimTypes.NameIdentifier`. The API rejects requests without a valid user identifier, copies the identifier into the response, command, and queued work item, and filters both recent and individual run reads by that owner. A run belonging to another user is returned as not found rather than revealing that it exists. The in-memory queue/store remains the MVP implementation; when durable storage is introduced, `UserId` must remain a persisted field and part of every read predicate rather than falling back to unscoped lookups.

### Thread-safe in-memory MVP persistence
The singleton in-memory repositories use `ConcurrentDictionary` and synchronize compound operations where a single concurrent-dictionary operation is not enough. User email and identifier insertion is protected as one operation, portfolio repository updates and query materialization are protected by a repository lock, and the `Portfolio` aggregate synchronizes position duplicate-check/add and exposes a materialized collection snapshot. Market-data queries materialize values before filtering, while concurrent upserts retain the existing symbol/date/source deduplication behavior. These are process-local safeguards, not a substitute for PostgreSQL transactions and unique indexes.

### Bounded backtest execution
The API-hosted MVP worker consumes a bounded in-memory channel with capacity 100. Enqueue is intentionally non-blocking: a full channel produces a clear `503 Service Unavailable` response, and the HTTP request cancellation token is checked and propagated before accepting a work item. The worker continues to use the host stopping token, allowing graceful processing cancellation during shutdown while recording normal execution failures. Queued work and in-memory run state are lost on process restart and are not shared between API instances; before production or horizontal scaling, move run state and jobs to PostgreSQL with a durable queue/claim transaction (or an equivalent durable messaging service).

### Domain first
The most important rules live in entities and domain validations before they appear in controllers or infrastructure services.

### JWT configuration boundary
JWT settings are loaded and validated once by the API composition root through
`Infrastructure.Identity.JwtSettings`. The Development environment has a clearly
development-only fallback to keep the local MVP easy to run. All other environments
fail during startup when `Jwt:Key` is missing or shorter than 32 characters, so a
publicly known signing key cannot be used accidentally. Deployments provide
`Jwt__Key`, `Jwt__Issuer`, and `Jwt__Audience` through configuration or environment
variables; the secret is never logged. `JwtTokenService` receives the same validated
settings instance used by token validation, preventing signing and validation from
drifting apart.

## 8. Evolution path

The next maturity step for the project points toward:
- real PostgreSQL storage,
- formal backtesting,
- deterministic and reproducible metrics,
- more integration tests,
- stronger separation between jobs and API request flow.

The goal is to keep the MVP simple while the solution grows without losing clarity or traceability.

## 9. Final notes

This document should stay up to date as the solution evolves. The architecture should reflect the real state of the project rather than an idealized future version.

This project is focused on pragmatic, Domain-Driven Design (DDD) over theoretical over-engineering.
