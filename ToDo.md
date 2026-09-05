# MVP ToDo - PortfolioAnalytics API

## Estado actual

El flujo local demostrable funciona:

`registrar usuario -> iniciar sesión -> crear portfolio -> agregar posiciones -> consultar market data -> ejecutar backtest -> obtener resultado`

La base usa repositorios en memoria y todavía necesita cerrar correcciones de correctness y deuda de ejecución antes de sumar producto.

## Orden exacto de ejecución

### PR0 — Correctness fixes

Corregir cuatro bugs concretos e independientes: validación completa OHLCV, rechazo de rangos de fechas invertidos, parsing ISO `yyyy-MM-dd` determinista en market data y registro de usuario atómico para evitar duplicados por race condition. Son rápidos, no dependen de la arquitectura futura y afectan la corrección real.

**Archivos/clases:** `MarketDataPoint`, `MarketDataController`, `InMemoryUserRepository`, `RegisterUserHandler` y tests de contrato/concurrencia.

**Listo cuando:** cada entrada inválida devuelve el contrato esperado y dos registros concurrentes con el mismo email solo permiten crear una cuenta.

### PR1 — Pureza de dominio + maquina de estados

Mover el cálculo financiero a Domain, hacer `BacktestRun` la única fuente de estados y enlazar `PerformanceMetrics` con el run correcto. Eliminar también representaciones duplicadas de ownership/estado porque forman parte del mismo contrato.

**Archivos/clases:** entidades, servicios y enums de `PortfolioAnalytics.Domain`, handlers/DTOs de backtest y tests.

**Listo cuando:** los cálculos y transiciones tienen tests deterministas y ningún resultado puede perder su `BacktestRun.Id`.

### PR2 — Cancelación y shutdown

Propagar `CancellationToken` por toda la ejecución, definir una política explícita de cola y reconciliar trabajos ante shutdown. Mantener el worker dentro del API durante el MVP.

**Archivos/clases:** `BacktestExecutionQueue`, `BacktestExecutionWorker`, `BacktestRun`, store, configuración y tests.

**Listo cuando:** una ejecución termina en un estado válido ante éxito, error, cancelación, overload o shutdown.

### PR3 — Límites de recursos

Agregar límites explícitos para payloads, workload y duración de ejecución, y reemplazar la retención ilimitada del store por una política acotada.

**Archivos/clases:** opciones/configuración, validación de API, `BacktestExecutionStore`, queue y tests de límites/evicción.

**Listo cuando:** el consumo de memoria, CPU y trabajos pendientes queda limitado por configuración sin descartar ejecuciones activas.

### PR4 — Command real + concurrencia honesta

Mover la orquestación de backtests del controller a un command/handler de Application y reemplazar tests de concurrencia engañosos por casos con agregados independientes y conflictos reales.

**Archivos/clases:** `BacktestsController`, commands/handlers, store/queue, repositorios afectados y tests de concurrencia.

**Listo cuando:** HTTP solo traduce entrada/salida y las pruebas detectan actualizaciones perdidas o estados inválidos antes de persistir en base de datos.

## Feature siguiente

**Comparación de estrategias** (rebalanceo periódico vs. buy-and-hold). Va después de P0 porque es la promesa central de una Backtesting API, reutiliza el calculador de Domain ya testeado y permite una demo clara comparando métricas de dos estrategias.

## Más adelante, no ahora

- **PostgreSQL + EF Core:** cuando el modelo de estado esté probado y la persistencia durable sea necesaria.
- **Dashboard de métricas:** cuando existan resultados históricos consultables y una necesidad clara de UI.
- **Integraciones reales de market data:** cuando el contrato normalizado esté estable.
- **Procesamiento durable, worker separado y escalado:** ante necesidad de recuperación tras reinicios o múltiples instancias.
- **CI/CD, observabilidad y despliegue:** cuando la aplicación deje de ser local-only.



## Idea general del paso a paso 

- Cerrar PR0-PR4 (ya definidos) — sin esto, nada de lo siguiente va a valer la pena sobre una base inconsistente.
- Una estrategia nueva (rebalanceo periódico es la más simple de implementar reutilizando BacktestCalculator). Objetivo: validar que el modelo soporta comparación A/B, no acumular features.
- Persistencia real: Postgres + EF Core, ahora con dos tipos de estrategia y la maquina de estados ya limpia de P0. Migramos una vez, con datos suficientes para diseñar bien el schema.
- Visualización mínima: hacemos una página estática con Chart.js contra la API, mostrando historial de runs y comparación de métricas entre estrategias. Algo sencillo, no un frontend completo.
- Recién ahí, iterar features de a una: proveedor de market data real, más estrategias, filtros de comparación — cada una ya cae en un modelo persistente y visualizable, así que cada nueva feature se demuestra sola sin trabajo extra.