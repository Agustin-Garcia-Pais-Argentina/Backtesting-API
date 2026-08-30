# ToDo del MVP - PortfolioAnalytics API

Este roadmap combina funcionalidad y factibilidad. La prioridad se ordena por valor para el usuario y por la capacidad de entregar algo útil sin sobreconstruir la base.

## Estado actual (Agosto 2026)

La base del proyecto ya está funcional en varios bloques clave:

- autenticación JWT y protección de endpoints,
- registro/login de usuarios,
- portfolio y positions con reglas basadas en el dominio,
- market data MVP con consulta por símbolo y rango de fechas,
- repositorio en memoria como capa de validación del flujo.

Lo que todavía no está resuelto en la base funcional es:
- backtesting real sobre series históricas,
- métricas financieras reproducibles,
- persistencia real en PostgreSQL,
- tests automáticos de dominio e integración.

## P0 - Base del producto y lo que hace al MVP útil

### 1. Definir el dominio financiero mínimo
- Objetivo: dejar claro qué entidades y reglas de negocio son necesarias para un analizador de portafolios.
- Quién: backend + arquitecto del producto.
- Cómo: definir entidades como User, Portfolio, Position, MarketDataPoint, StrategyDefinition, BacktestRun y PerformanceMetrics.
- Dónde: `src/PortfolioAnalytics.Domain/`
- Afecta: todo el proyecto porque establece el modelo base.
- Mejoras futuras: agregar Value Objects para Money, DateRange, StrategyParameters y reglas más estrictas de validación financiera.

### 2. Crear la estructura de solución y proyectos .NET
- Objetivo: dejar el repositorio listo para escalar sin mezclar responsabilidades.
- Quién: backend.
- Cómo: crear proyectos `Domain`, `Application`, `Infrastructure`, `Api`, `Worker`, `Contracts`, `Shared` y `tests`.
- Dónde: `src/` y `tests/`
- Afecta: toda la organización del código y el flujo de compilación.
- Mejoras futuras: separar paquetes por feature o by-context cuando el proyecto crezca y haya más especialización.

### 3. Configurar PostgreSQL y entorno local con Docker
- Objetivo: habilitar una base real para desarrollo y pruebas de integración.
- Quién: backend / DevOps.
- Cómo: definir `docker-compose.yml` con PostgreSQL y variables de entorno mínimas.
- Dónde: raíz del proyecto y `docker/`
- Afecta: desarrollo local, tests y despliegue.
- Mejoras futuras: sumar Redis, pgAdmin, observabilidad y scripts de seed de datos.

### 4. Implementar autenticación con JWT
- Objetivo: proteger endpoints de usuarios, carteras y resultados.
- Quién: backend.
- Cómo: crear registro/login, generación de JWT, validación de claims y middleware de autenticación.
- Dónde: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Infrastructure/Identity/`
- Afecta: seguridad y acceso de los usuarios.
- Mejoras futuras: refresh tokens, rotación de claves, roles y auditoría de sesiones.

### 5. Implementar CRUD de usuarios y portafolios
- Objetivo: permitir que un usuario cree una cartera y la administre.
- Quién: backend + API.
- Cómo: crear endpoints de portfolio y validaciones de nombre, usuario y estado.
- Dónde: `src/PortfolioAnalytics.Application/`, `src/PortfolioAnalytics.Api/Controllers/`
- Afecta: el caso de uso central del producto.
- Mejoras futuras: compartir carteras, permisos por usuario, tags de riesgo y snapshots de cartera.

### 6. Implementar posiciones dentro del portafolio
- Objetivo: representar activos dentro de una cartera con cantidad y costo base.
- Quién: dominio + aplicación.
- Cómo: modelar `Position` y servicios para agregar, actualizar y eliminar posiciones.
- Dónde: `src/PortfolioAnalytics.Domain/Entities/` y `src/PortfolioAnalytics.Application/`
- Afecta: la funcionalidad financiera principal.
- Mejoras futuras: soportar instrumentos complejos, lotes, costos de transacción y rebalancing automático.

### 7. Implementar ingestión de datos históricos de mercado
- Objetivo: poblar series temporales para volver comparables las estrategias.
- Quién: aplicación + infraestructura.
- Cómo: crear servicio de sincronización, normalización y deduplicación por símbolo + fecha + fuente.
- Dónde: `src/PortfolioAnalytics.Infrastructure/ExternalServices/`, `src/PortfolioAnalytics.Application/`
- Afecta: el backtest y la calidad de los resultados.
- Mejoras futuras: soportar más proveedores, fallback de fuentes, validación de feriados y enriquecimiento de datos.

### 8. Crear la primera estrategia de backtest
- Objetivo: demostrar valor real con una estrategia útil y validable.
- Quién: dominio + aplicación.
- Cómo: empezar con SMA crossover o buy-and-hold, con parámetros configurables.
- Dónde: `src/PortfolioAnalytics.Domain/`, `src/PortfolioAnalytics.Application/Services/`, `src/PortfolioAnalytics.Worker/`
- Afecta: la capacidad del sistema de entregar análisis de rendimiento.
- Mejoras futuras: incluir estrategia de rebalanceo, momentum, mean-reversion, optimización de parámetros y backtest multi-asset.

### 9. Ejecutar backtests en segundo plano
- Objetivo: que la API no se bloquee cuando se corre un cálculo pesado.
- Quién: backend + worker.
- Cómo: un endpoint retorna 202 Accepted con un job ID y un worker procesa el cálculo.
- Dónde: `src/PortfolioAnalytics.Api/`, `src/PortfolioAnalytics.Worker/`, `src/PortfolioAnalytics.Infrastructure/BackgroundJobs/`
- Afecta: la experiencia de usuario y la escalabilidad de la API.
- Mejoras futuras: cola de trabajos, retries, cancelación, event-driven processing y resultados en almacenamiento externo.

### 10. Guardar resultados de backtest y métricas clave
- Objetivo: persistir los resultados para compararlos después.
- Quién: infraestructura + aplicación.
- Cómo: guardar resumen de la corrida, estrategia, parámetros y métricas calcularizadas.
- Dónde: `src/PortfolioAnalytics.Domain/Entities/`, `src/PortfolioAnalytics.Infrastructure/Repositories/`
- Afecta: la utilidad del producto porque permite comparar estrategias en el tiempo.
- Mejoras futuras: guardar series de equity curve, trade log y snapshots por fecha.

### 11. Exponer endpoints REST del MVP
- Objetivo: dejar las funcionalidades listas para un frontend o cliente consumidor.
- Quién: API.
- Cómo: endpoints para login, portfolio, market data y backtest status/results.
- Dónde: `src/PortfolioAnalytics.Api/Controllers/`
- Afecta: toda la experiencia de uso.
- Mejoras futuras: versionado de API, paginación, filtros avanzados y contratos estables.

## P1 - Mejora de valor y producto

### 12. Mostrar métricas de rendimiento en un dashboard
- Objetivo: que el usuario vea resultados comprensibles.
- Quién: frontend + API + analytics.
- Cómo: devolver summary metrics para que el cliente los represente en gráficos.
- Dónde: `src/PortfolioAnalytics.Api/` y `client/`
- Afecta: la adopción del producto y la toma de decisiones.
- Mejoras futuras: comparativa por benchmark, heatmaps, equity curves, drawdown charts y export CSV/PDF.

### 13. Comparar estrategias entre sí
- Objetivo: permitir evaluar cuál estrategia funciona mejor en un mismo portafolio.
- Quién: backend + frontend.
- Cómo: guardar varias corridas y comparar sus métricas.
- Dónde: `src/PortfolioAnalytics.Application/Queries/` y `src/PortfolioAnalytics.Api/`
- Afecta: la utilidad analítica del producto.
- Mejoras futuras: optimización automática de parámetros y rankings por métricas ponderadas.

### 14. Implementar tests unitarios del dominio y casos de uso
- Objetivo: asegurar que las reglas del negocio se mantengan estables.
- Quién: backend.
- Cómo: usar xUnit/NUnit y pruebas para validación de portafolios, posiciones y métricas.
- Dónde: `tests/PortfolioAnalytics.UnitTests/`
- Afecta: calidad de la base y reducción de regresiones.
- Mejoras futuras: tests de propiedad, golden files y fixtures con datasets reales.

### 15. Implementar pruebas de integración con PostgreSQL real
- Objetivo: asegurar que la base, repositorios y API funcionan junto.
- Quién: backend.
- Cómo: usar Testcontainers para levantar PostgreSQL durante el CI.
- Dónde: `tests/PortfolioAnalytics.IntegrationTests/`
- Afecta: la confiabilidad del producto.
- Mejoras futuras: pruebas end-to-end con cliente y pipeline CI/CD completo.

## P2 - Escala, robustez y producción

### 16. Agregar observabilidad y logging estructurado
- Objetivo: tener trazabilidad para fallos y ejecuciones pesadas.
- Quién: backend / DevOps.
- Cómo: logging estructurado, health checks, traces y métricas básicas.
- Dónde: `src/PortfolioAnalytics.Api/` y `src/PortfolioAnalytics.Worker/`
- Afecta: operabilidad en entorno real.
- Mejoras futuras: OpenTelemetry, Prometheus y alertas inteligentes.

### 17. Optimizar consultas de series temporales
- Objetivo: manejar volumen grande de datos sin saturar memoria ni rendimiento.
- Quién: infraestructura.
- Cómo: usar Dapper para consultas pesadas, índices, particionado por fecha y stratégie de caching.
- Dónde: `src/PortfolioAnalytics.Infrastructure/DataAccess/` y repositorios.
- Afecta: rendimiento y escalabilidad.
- Mejoras futuras: usar TimescaleDB o almacenamiento especializado para series temporales.

### 18. Añadir autenticación avanzada y seguridad
- Objetivo: preparar el producto para una base más segura.
- Quién: backend.
- Cómo: refresh tokens, expiración, revocación, validaciones más fuertes y políticas de contraseñas.
- Dónde: `src/PortfolioAnalytics.Infrastructure/Identity/` y API.
- Afecta: confianza del usuario y cumplimiento.
- Mejoras futuras: 2FA, roles, permisos por portfolio y tenant-aware access.

### 19. Preparar arquitectura para múltiples estrategias y fuentes de datos
- Objetivo: hacer el sistema extensible.
- Quién: backend.
- Cómo: abstraer proveedores y definiciones de estrategia con interfaces y contratos.
- Dónde: `src/PortfolioAnalytics.Contracts/` y `src/PortfolioAnalytics.Infrastructure/`
- Afecta: extensibilidad del sistema.
- Mejoras futuras: incorporar más fuentes de datos, estrategias personalizadas y benchmarking.

### 20. Preparar despliegue y CI/CD
- Objetivo: dejar una ruta de release clara.
- Quién: DevOps + backend.
- Cómo: pipeline de build, tests, linting, empaquetado y despliegue Docker.
- Dónde: `.github/workflows/`, `docker/`, `docker-compose.yml`
- Afecta: estabilidad y capacidad de entrega continua.
- Mejoras futuras: ambientes de staging y producción, quality gates y rollout automatizado.

## Criterio de priorización

Se ordenan primero:
- tareas que entregan valor funcional al usuario,
- tareas que son viables dentro del MVP,
- tareas que reducen riesgo técnico sin sobreconstruir.

Se dejan para después:
- mejoras de operación,
- escalabilidad avanzada,
- seguridad y extensibilidad por encima de un MVP probado.

## Recomendación práctica

Para mantener el proyecto útil y ejecutable, el orden sugerido es:
1. dominio + estructura,
2. auth,
3. portfolios + positions,
4. market data,
5. backtest base,
6. resultados + métricos,
7. tests,
8. dashboard,
9. producción y escala.

Esto permite empezar a entregar algo concreto sin perder de vista la arquitectura.
