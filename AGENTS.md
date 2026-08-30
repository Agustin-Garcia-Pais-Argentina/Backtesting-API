# AGENTS.md

This file defines the rules for planning and code creation in this project.

## 1. General project purpose

This repository is focused on building a useful MVP for financial analysis and portfolio management, with emphasis on:
- user management,
- portfolio and position management,
- market data synchronization,
- backtesting execution,
- calculation of relevant financial metrics,
- REST API exposure for a frontend client.

The priority is not to build a complex architecture, but to deliver a useful, maintainable, and verifiable product.

## 2. Planning rules

### 2.1. Prioritize utility over architectural aesthetics
- Before implementing, ask: does this add real functional value to the user?
- Do not add layers, patterns, or abstractions without a concrete need.
- Avoid "demo architecture" that does not solve a real problem.

### 2.2. Keep the MVP scope clear
- The project must advance in stages.
- Each change should fit an actual MVP use case.
- If a feature is not required for the MVP, it should be documented as a future improvement.

### 2.3. Solve what creates immediate value first
The recommended order is:
1. base domain,
2. authentication,
3. portfolio + positions,
4. market data,
5. base backtesting,
6. results and metrics,
7. tests,
8. dashboard,
9. production and scaling.

### 2.4. Keep plans small and executable
- Split tasks into concrete and verifiable steps.
- Each change should be explainable in a few phrases: objective, scope, files involved, validation.
- Do not create tasks that are too large or too abstract.

### 2.5. Ask for clarification when a decision affects the product
- If there is uncertainty about scope, UX, financial rules, or architecture, ask before implementing.
- When more than one option is reasonable, choose the simplest and most useful one for the MVP.

## 3. Development rules

### 3.1. Respect the layered architecture
- `Domain`: entities, value objects, repository interfaces, and pure business logic.
- `Application`: use cases, commands, queries, handlers, validation, and DTOs.
- `Infrastructure`: EF Core, repositories, HTTP clients, jobs, and technical authentication.
- `Api`: controllers, middleware, Swagger, and REST exposure.
- `Worker`: heavy asynchronous tasks.
- `Shared` and `Contracts`: shared utilities and contracts.

### 3.2. Do not mix responsibilities
- The API should not contain real business logic.
- The domain must not depend on EF Core, ASP.NET, or infrastructure.
- Infrastructure should not decide core business rules.

### 3.3. Prefer clarity over sophistication
- Use simple, direct, and consistent naming.
- Avoid overengineering with "pretty patterns".
- Prioritize maintainability and readability.

### 3.4. Keep the software testable
- Each major feature should be validated with unit or integration tests.
- Do not leave critical logic untested.
- Avoid coupling the code to tests, but design it so it can be tested.

### 3.5. Use the right technology for the problem
- .NET + ASP.NET Core + PostgreSQL + EF Core is the recommended base.
- Use Dapper only when massive time-series queries truly require it.
- Apply CQRS only when real complexity justifies it.
- Use a worker only for actual heavy asynchronous tasks.

### 3.6. Comment the code properly
- Add comments at code-block level, explaining purpose and how it works in a simple technical style.
- Add a brief file-level description at the top of each file explaining its purpose.
- Keep documentation current whenever an important architectural or workflow change alters project context.

## 4. Code rules

### 4.1. Keep improvements minimal and precise
- Make small, direct changes.
- Do not rewrite large sections unless necessary.
- Do not solve unrelated problems while working on the current task.

### 4.2. Maintain project consistency
- Files, folders, and names should follow this style:
  - PascalCase for classes, enums, and public methods,
  - camelCase for variables and parameters,
  - descriptive English folder names,
  - `PortfolioAnalytics.` as the project and subproject prefix.

### 4.3. Do not over-document the code
- Comments should only add real technical clarification.
- Prefer expressive names over excessive comments.

### 4.4. Do not leave provisional code in production
- Do not leave `TODO` items without serious context.
- Do not leave unresolved business placeholders.
- Do not leave noisy logs or debugging cruft.

### 4.5. Validation rules before closing a task
- Verify compilation when applicable.
- Run the smallest relevant test that covers the change.
- If the change affects the API, validate the relevant flow.
- If the change affects financial logic, review the calculation carefully.

## 5. Financial domain-specific rules

- Metrics must be consistent and reproducible.
- Time series should be normalized before calculation.
- Deduplication by symbol + date + source must be considered.
- Backtests must be reproducible based on the parameters used.
- Financial calculations should prioritize clarity over "mathematical magic".

## 6. Rules for implementation decisions

### If there is doubt between two paths:
- choose the simplest solution,
- choose the one that solves the problem without unnecessary complexity,
- and choose the one that allows progress to the next MVP step.

### If a feature can wait:
- leave it in the backlog as a future improvement,
- do not block the MVP with it.

## 7. Communication rules in the project

- Explain why before explaining how.
- Show the consequences of architectural decisions.
- Explain the functional impact of each change.
- Keep documentation up to date when the architecture or workflow changes.

## 8. Final quality standard

A good implementation is considered one that:
- solves a real user problem,
- keeps the code understandable,
- avoids complexity without need,
- is validated with tests or practical validation,
- and fits the MVP roadmap.

## 9. Exception

If the user explicitly requests something different, that request takes priority over these rules.
