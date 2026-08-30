# Architecture Overview

Este documento funciona como registro vivo de la arquitectura del proyecto. Sirve para:
- visualizar cómo está armada la solución,
- entender qué hace cada capa,
- registrar decisiones de diseño,
- mantener una referencia educativa a medida que el proyecto avance.

Se recomienda actualizar este archivo cada vez que cambie la arquitectura principal, el flujo de funcionamiento o la estructura del sistema.

## 1. Propósito general del sistema

El proyecto busca convertirse en una API de análisis financiero y gestión de portafolios, orientada a:
- gestionar usuarios,
- crear y administrar carteras,
- sincronizar datos de mercado,
- evaluar estrategias de inversión con backtesting,
- calcular métricas clave de rendimiento y riesgo,
- exponer funcionalidad a un cliente frontend o a otros consumidores.

El enfoque principal del MVP es resolver un caso real y útil: 
“un usuario puede gestionar una cartera, cargar datos de mercado y ejecutar simulaciones de estrategias sobre su portafolio”.

## 2. Arquitectura general

La solución está pensada con una separación por capas para mantener claridad y permitir evolución sin mezclar responsabilidades.

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
│ - Backtest execution                                                │
│ - Market sync or asynchronous processing                             │
└──────────────────────────────────────────────────────────────────────┘
```

## 3. Qué hace cada parte

### 3.1. Domain
Es la capa más pura del proyecto. No conoce ASP.NET, EF Core ni la API.

Responsabilidades:
- definir entidades del negocio,
- encapsular reglas de negocio simples,
- dar estructura al dominio financiero,
- definir contratos de repositorios,
- mantener excepciones y validaciones de dominio.

Ejemplos actuales:
- `User`
- `Portfolio`
- `Position`
- `MarketDataPoint`
- `StrategyDefinition`
- `BacktestRun`
- `PerformanceMetrics`

### 3.2. Application
Es la capa de casos de uso.

Responsabilidades:
- ejecutar acciones del producto,
- preparar comandos y consultas,
- orquestar validaciones y llamadas a servicios,
- coordinar la lógica de negocio que no pertenece a una entidad aislada,
- preparar DTOs para la capa API.

Ejemplos aproximados:
- `CreatePortfolioCommand`
- `AddPositionCommand`
- `RunBacktestCommand`
- `GetPortfolioSummaryQuery`
- `CreatePortfolioHandler`

### 3.3. Infrastructure
Es la capa de implementación técnica.

Responsabilidades:
- persistencia con EF Core,
- repositorios concretos,
- acceso a fuentes externas de mercado,
- JWT o seguridad técnica,
- job processing y tareas pesadas,
- cualquier dependencia externa.

Ejemplos:
- `AppDbContext`
- `PortfolioRepository`
- `MarketDataRepository`
- `YahooFinanceClient`
- `JwtTokenService`

### 3.4. Api
Es la capa de exposición de funcionalidad.

Responsabilidades:
- recibir HTTP requests,
- mapear DTOs,
- invocar casos de uso,
- devolver respuestas estándar,
- manejar errores globalmente,
- documentar endpoints con Swagger.

No debería contener lógica de negocio compleja; solo orquesta.

### 3.5. Worker
Es la capa para trabajos pesados y asíncronos.

Responsabilidades:
- ejecutar backtests complejos,
- cargar datos masivos,
- correr tareas en background,
- no bloquear la API principal.

Esto ayuda a mantener la API reactiva y a facilitar escalado cuando la carga crece.

## 4. Flujo principal del MVP

### 4.1. Flujo de usuario y portafolio
```text
Usuario -> API -> Application -> Domain -> Repository -> PostgreSQL
```

Ejemplo:
- usuario crea un portfolio,
- controldor recibe request,
- handler valida,
- domain crea la entidad,
- repositorio la persiste,
- response vuelve al cliente.

### 4.2. Flujo de sincronización de datos de mercado
```text
API -> Application -> Infrastructure Repository -> In-memory store
```

Objetivo:
- recibir una carga de precios históricos,
- validarlos con el dominio,
- guardar la serie para análisis posterior,
- dejar la estructura lista para cambiar a una fuente persistente real en el futuro.

En la versión actual, la infraestructura usa un repositorio en memoria para validar el flujo sin bloquear el MVP por una base de datos completa.

### 4.3. Flujo de backtesting
```text
API -> Application -> Worker -> Strategy engine -> Market data -> Metrics -> Persist result
```

Objetivo:
- tomar una estrategia de inversión,
- simular rendimiento sobre un historial,
- calcular métricas relevantes,
- guardar el resultado y permitir comparación posterior.

## 5. Entidades principales del dominio

### User
Representa a un usuario autenticado.

Atributos clave:
- Id
- Email
- FullName
- CreatedAt

### Portfolio
Representa una cartera del usuario.

Atributos clave:
- Id
- UserId
- Name
- Positions

### Position
Representa un activo dentro de una cartera.

Atributos clave:
- Symbol
- Quantity
- AverageCost
- AssetType

### MarketDataPoint
Representa un punto de precio histórico.

Atributos clave:
- Symbol
- Date
- Open
- High
- Low
- Close
- Volume
- Source

### StrategyDefinition
Representa una estrategia de inversión.

Atributos clave:
- Name
- Type
- ParametersJson

### BacktestRun
Representa una corrida de backtesting.

Atributos clave:
- UserId
- PortfolioId
- StrategyId
- Status
- StartedAt
- FinishedAt
- ResultSummaryJson

### PerformanceMetrics
Representa el resultado cuantitativo del backtest.

Atributos clave:
- TotalReturn
- AnnualizedReturn
- MaxDrawdown
- SharpeRatio
- Volatility
- TradeCount

## 6. Decisiones de diseño actuales

### Separación por capas
Se usa para aumentar claridad y mantener una base sostenible.

### Dominio claro
El modelo financiero se define en Domain para que las entidades no dependan del framework ni de la infraestructura.

### Worker dedicado para backtests
La lógica pesada se mueve a una capa independiente para no bloquear la API.

### PostgreSQL como base principal
Es una base sólida para persistir usuarios, portafolios, series históricas y resultados.

### Enfoque MVP
El sistema se diseña para resolver un caso práctico, no para ser una plataforma financiera “perfecta” desde el inicio.

## 7. Qué no queremos en esta fase

- mezclar lógica de negocio en controladores,
- poner EF Core, HTTP y JWT en el dominio,
- overengineering con CQRS completo sin necesidad,
- hacer múltiples proveedores externos antes de validar el MVP,
- usar workers sin una tarea realmente demandante,
- construir un frontend complejo antes de validar el backend y las métricas.

## 8. Cómo evoluciona la arquitectura

A medida que el proyecto crezca, esta arquitectura puede evolucionar hacia:
- más servicios dedicados,
- queries optimizadas con Dapper,
- más pruebas de integración,
- mejor observabilidad,
- worker más robusto,
- mayor separación entre estrategia y análisis financiero,
- dashboard y comparación de resultados.

Sin embargo, en el MVP la prioridad es mantener la solución simple, entendible y útil.

## 9. Estado actual del proyecto

Actualmente la solución ya está creada con la estructura base, un dominio claro y una primera capa funcional de negocio. El proyecto ya incluye:

- autenticación de usuarios con JWT,
- registro y login,
- API protegida por token,
- portfolios y posiciones con reglas básicas de dominio,
- market data MVP con repositorio en memoria,
- endpoints para sincronizar y consultar series históricas por símbolo.

Esto quiere decir que, a nivel de arquitectura, el sistema ya validó el flujo principal de usuario + portfolio + market data en local, aunque todavía no se ha incorporado la persistencia definitiva en PostgreSQL ni el motor de backtesting real.

Las próximas áreas a completar son:
- backtest runner base,
- cálculo de métricas financieras reproducibles,
- tests automáticos del dominio y de integración,
- persistencia real en PostgreSQL y migraciones,
- separación de jobs pesados en worker.

## 10. Cómo actualizar este documento

Cada vez que ocurra uno de estos cambios, este archivo debe actualizarse:
- se agrega una nueva capa o servicio importante,
- cambia el flujo de trabajo principal,
- se introduce un nuevo motor de tareas,
- cambia la infraestructura de persistencia,
- se incorpora una nueva funcionalidad clave del producto.

Formato recomendado del update:
- describir el cambio,
- indicar por qué se hizo,
- mostrar la nueva estructura del flujo,
- explicar impacto sobre dominio / infraestructura / API.

## 11. Registro educativo

Este archivo sirve además como documento pedagógico para entender:
- cómo se separan responsabilidades en una solución .NET,
- cómo la arquitectura guía el desarrollo,
- qué ventajas tiene mantener capas claras,
- cómo evoluciona una idea de producto hacia un sistema más robusto.

## 12. Resumen corto

La arquitectura del proyecto intenta ser:
- simple,
- útil,
- extensible,
- y centrada en el problema de negocio real: análisis financiero y backtesting de estrategias.

La idea central es que cada capa tenga una misión clara, sin mezclar lógica técnica con lógica de negocio ni dejar la API como contenedor de todo.
