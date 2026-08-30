# Project plan

## Current status

This repository is a functional MVP foundation for a financial analytics backend. The current implementation already includes:

- domain model for users, portfolios, positions, and market data
- JWT-based authentication and protected endpoints
- in-memory repositories for local validation and development speed
- sample market-data ingestion and time-series queries by symbol
- unit tests for the core business rules and key flows

This is not a production-ready system yet, but it is a real progression toward a useful product and is good material for a public GitHub repository.

## Recommended GitHub strategy

Use the repository publicly as a learning and progress-tracking project. The objective is to show a real engineering workflow, not to pretend the project is fully finished.

Recommended flow:

1. Keep `main` as the stable branch.
2. Create a feature branch for each change (`feature/auth-improvements`, `feature/backtesting-base`, etc.).
3. Open pull requests with a clear description, screenshots if needed, and acceptance notes.
4. Merge only after review and validation.
5. Use issues to track backlog items and architecture decisions.

This fits GitHub Flow well and makes the project look intentional and professional.

## Near-term roadmap

### Phase 1: MVP stability
- expand unit and integration tests
- validate JWT and API contracts more thoroughly
- capture the real API behavior in examples and docs

### Phase 2: financial core
- implement a basic strategy/backtest engine
- add metrics: return, volatility, drawdown, Sharpe, CAGR
- define how market-data sources will be normalized

### Phase 3: persistence and production readiness
- move from in-memory storage to PostgreSQL + EF Core
- add migration and repository patterns for real data durability
- introduce environment configuration and deployment basics

## Notes for the public repo

- Keep README honest about the MVP status.
- Show progress in small chunks instead of waiting for a perfect finish.
- Tag milestones when meaningful progress is reached.
- Use the public repo as a portfolio piece and as a learning log.

## Recommendation

Yes: I would publish it now, but with a clear message that it is an MVP in continuous evolution. That is better than waiting for "perfect" completion, especially if the goal is to demonstrate engineering discipline and gradual delivery.
