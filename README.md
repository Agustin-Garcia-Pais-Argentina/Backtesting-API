# PortfolioAnalytics API

Este proyecto es un motor de backtesting y análisis financiero expuesto como una API RESTful. Está pensado para ayudar a usuarios a simular estrategias de inversión sobre datos históricos, evaluar su desempeño y gestionar carteras de activos de forma reproducible y ordenada.

El objetivo es trasladar la lógica analítica que suele vivir en scripts locales hacia un entorno más mantenible, seguro y escalable. El sistema permite sincronizar datos de mercado, modelar portafolios, ejecutar backtests y medir métricas clave como rentabilidad, drawdown, Sharpe y volatilidad.

## ¿Qué es?

Es un backend orientado a Backtesting-as-a-Service: una API que permite ejecutar simulaciones de inversión sobre series históricas, comparar estrategias y mantener el estado de un portafolio de forma estructurada.

## ¿A quiénes les sirve?

Está orientado a:
- desarrolladores de aplicaciones fintech,
- analistas financieros,
- usuarios que desean validar ideas de inversión sin depender de Excel, notebooks aislados o scripts manuales,
- equipos que necesitan un motor analítico reproducible y fácil de integrar.

## ¿Qué problema resuelve?

La validación de estrategias de inversión normalmente ocurre en scripts locales que consumen archivos poco ordenados, saturan la memoria y no son reproducibles ni escalables. Este proyecto busca estandarizar el flujo de datos, gestionar portfolios y permitir ejecutar simulaciones pesadas sin bloquear la API ni depender de una computadora local.

## ¿Qué hace?

- Ingesta datos de mercado desde fuentes externas.
- Normaliza y almacena series históricas.
- Gestiona portfolios y posiciones de activos.
- Ejecuta backtests de estrategias sobre datos históricos.
- Calcula métricas relevantes de rendimiento y riesgo.
- Expone el sistema a través de una API REST para frontend o integraciones.

## Arquitectura orientada al MVP

La solución se estructura con capas bien definidas:
- Domain: entidades y reglas del negocio financiero.
- Application: casos de uso y workflows de negocio.
- Infrastructure: persistencia, clientes externos y autenticación técnica.
- API: exposición HTTP.
- Worker: procesamiento pesado en segundo plano.

La intención no es construir una plataforma financiera “perfecta” desde el primer día, sino entregar un producto útil, mantenible y verificable.

## Objetivo del proyecto

Convertir scripts analíticos de inversión en un producto útil y mantenible para:
- crear y gestionar carteras de inversión,
- sincronizar precios históricos de activos,
- definir estrategias simples de inversión,
- ejecutar backtests sobre datos históricos,
- visualizar métricas clave de rendimiento y riesgo.

La idea es construir un MVP útil, no una plataforma financiera “de lujo” desde el primer día.

## MVP propuesto y estado real

El mínimo viable para que el proyecto tenga valor real es:
- Autenticación de usuarios con JWT. [Hecho en la base actual]
- Creación y gestión de portafolios. [Hecho en la base actual]
- Registro de posiciones (activo, cantidad, costo promedio). [Hecho en la base actual]
- Carga o sincronización de series históricas de precios. [Hecho en la base actual con repositorio en memoria]
- Ejecución de backtests para estrategias simples. [Siguiente bloque]
- Cálculo de métricas de rendimiento y riesgo. [Siguiente bloque]
- Persistencia central en PostgreSQL. [Próximo paso de maduración]
- API REST para integrar con un cliente frontend. [En base funcional]

## Stack recomendado

- .NET 8 / C#
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- Dapper (solo si la lectura de series temporales lo exige)
- MediatR (opcional, sólo si el proyecto crece) 
- Docker + Docker Compose
- Hangfire o un worker service opcional para tareas pesadas
- Testcontainers para integración

## Estructura del repositorio

```text
.
├── README.md
├── ToDo.md
├── docker-compose.yml
├── .gitignore
├── src/
│   ├── PortfolioAnalytics.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── Exceptions/
│   ├── PortfolioAnalytics.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Handlers/
│   │   ├── DTOs/
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Abstractions/
│   ├── PortfolioAnalytics.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   ├── DataAccess/
│   │   ├── ExternalServices/
│   │   ├── BackgroundJobs/
│   │   └── Identity/
│   ├── PortfolioAnalytics.Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── Program.cs
│   ├── PortfolioAnalytics.Worker/
│   │   ├── Services/
│   │   └── Program.cs
│   ├── PortfolioAnalytics.Shared/
│   ├── PortfolioAnalytics.Contracts/
│   └── ...
├── tests/
│   ├── PortfolioAnalytics.UnitTests/
│   └── PortfolioAnalytics.IntegrationTests/
├── client/
│   ├── src/
│   └── public/
└── docker/
```

## Cómo pensar el dominio

El núcleo del producto gira en torno a estas entidades:
- Usuario
- Portfolio
- Position
- Asset
- Trade
- MarketDataPoint
- StrategyDefinition
- BacktestRun
- PerformanceMetrics

Las reglas importantes del negocio deben vivir en el dominio, no en la API ni en la infraestructura.

## Flujos funcionales principales

### 1. Crear y manejar un portafolio
- Un usuario crea una cartera.
- Agrega activos con cantidad y costo promedio.
- Puede modificar o eliminar posiciones.

### 2. Sincronizar datos de mercado
- Se obtienen precios históricos.
- Se normalizan y se guardan.
- Se validan duplicados por símbolo + fecha + fuente.

### 3. Ejecutar backtest
- Se toma una estrategia definida.
- Se ejecuta sobre históricos de mercado.
- Se calculan métricas (retorno, drawdown, Sharpe, etc.).
- Se guarda el resultado para comparación posterior.

## Métricas clave del MVP

- CAGR
- Retorno total
- Drawdown máximo
- Sharpe ratio
- Volatilidad
- Número de trades
- Compare vs benchmark

## Roadmap sugerido

### Fase 1: MVP funcional
- Auth JWT
- Portfolio CRUD
- Position management
- Market data sync
- Backtest básico
- Resultados y métricas

### Fase 2: Productización
- Comparación entre estrategias
- Historial de corridas
- Mejoras de observabilidad
- Export de resultados

### Fase 3: Escala y robustez
- Worker dedicado
- Dapper para lecturas masivas
- TimescaleDB o particionamiento de series
- CI/CD con Testcontainers

## Cómo arrancar localmente

1. Clonar el repositorio.
2. Ajustar variables de entorno.
3. Levantar PostgreSQL con Docker Compose.
4. Ejecutar la API.
5. Ejecutar el worker si la tarea pesada se mueve a un proceso separado.

Ejemplo conceptual:

```bash
docker compose up -d

dotnet restore

dotnet build

dotnet run --project src/PortfolioAnalytics.Api
```

## Estado actual

El proyecto ya no está solo en la fase de definición. En esta etapa actual se tiene una base funcional de:

- autenticación con JWT,
- registro y login de usuarios,
- creación de portfolios y posiciones,
- API protegida por token,
- almacenamiento de series históricas de mercado con repositorio en memoria.

Esto significa que la plataforma ya puede validar el flujo principal del negocio en local, aunque todavía no se ha incorporado la capa de persistencia definitiva ni el motor de backtesting real.

El siguiente bloque de trabajo es: backtesting base + métricas + tests automáticos.

## Contribuir

Las contribuciones se priorizarán según el roadmap y la factibilidad técnica del momento. El criterio base es:
- impacto funcional real,
- simplicidad de implementación,
- claridad del modelo de dominio,
- capacidad de validar con pruebas.

## Notas

Este proyecto está pensado como una pieza técnica útil para análisis financiero y estrategia de inversión, no como un “demo de arquitectura” sin valor operativo.
