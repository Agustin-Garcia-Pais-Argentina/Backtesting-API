# AGENTS.md

Este archivo define las reglas para la planificación y la creación de código en este proyecto.

## 1. Propósito general del proyecto

Este repositorio está orientado a construir un MVP útil para análisis financiero y gestión de portafolios, con foco en:
- gestión de usuarios,
- gestión de carteras y posiciones,
- sincronización de datos de mercado,
- ejecución de backtests,
- cálculo de métricas financieras relevantes,
- API REST para un cliente frontend.

La prioridad no es “hacer una arquitectura compleja”, sino entregar un producto útil, mantenible y verificable.

## 2. Reglas de planificación

### 2.1. Priorizar utilidad antes que estética arquitectónica
- Antes de implementar, responder: ¿esto aporta valor funcional real al usuario?
- No agregar capas, patrones o abstracciones sin necesidad concreta.
- Evitar “demo architecture” que no resuelva problema real.

### 2.2. Mantener el alcance del MVP claro
- El proyecto debe avanzar en etapas.
- Cada cambio debe encajar con un caso de uso real del MVP.
- Si una funcionalidad no es necesaria para el MVP, debe dejarse documentada como mejora futura.

### 2.3. Resolver primero lo que genera valor inmediato
El orden recomendado es:
1. dominio base,
2. autenticación,
3. portfolio + posiciones,
4. market data,
5. backtest base,
6. resultados y métricas,
7. tests,
8. dashboard,
9. producción y escalado.

### 2.4. Hacer planes pequeños y ejecutables
- Dividir tareas en pasos concretos y verificables.
- Cada cambio debe poder explicarse en pocas frases: objetivo, alcance, archivos involucrados, validación.
- No crear tareas demasiado grandes o demasiado abstractas.

### 2.5. Pedir aclaración cuando la decisión afecta el producto
- En caso de duda sobre alcance, UX, reglas financieras o arquitectura, pedir una decisión antes de implementar.
- Si hay más de una opción razonable, elegir la más simple y útil para el MVP.

## 3. Reglas de desarrollo

### 3.1. Respetar la arquitectura por capas
- `Domain`: entidades, value objects, interfaces de repositorio, reglas de negocio puras.
- `Application`: casos de uso, commands, queries, handlers, validaciones y DTOs.
- `Infrastructure`: EF Core, repositorios, clientes HTTP, jobs, autenticación técnica.
- `Api`: controladores, middleware, Swagger, exposición REST.
- `Worker`: tareas pesadas y asíncronas.
- `Shared` y `Contracts`: utilidades y contratos compartidos.

### 3.2. No mezclar responsabilidades
- La API no debe contener lógica de negocio real.
- El dominio no debe depender de EF Core, ASP.NET ni de infraestructura.
- La infraestructura no debe decidir reglas clave del negocio.

### 3.3. Preferir claridad sobre sofisticación
- Naming simple, directo y consistente.
- Evitar overengineering por “patrones bonitos”.
- Priorizar mantenibilidad y legibilidad.

### 3.4. Mantener el software testeable
- Cada funcionalidad principal debe poder validarse con pruebas unitarias o de integración.
- No dejar lógica crítica sin pruebas.
- Evitar acoplar el código a pruebas, pero sí diseñarlo para que pueda probarse.

### 3.5. Usar tecnología apropiada al problema
- .NET + ASP.NET Core + PostgreSQL + EF Core es la base recomendada.
- Dapper solo si la consulta masiva de series temporales lo exige.
- CQRS solo si se justifica por complejidad real.
- Worker solo si hay tareas pesadas y asíncronas reales.

### 3.6 Comentar el codigo correctamente
- Comentar por bloques de codigo, su objetivo y como lo logran. De manera tecnica pero simple.
- Comentar al inicio de cada archivo el fin del mismo de manera coloquial.
- Mantener la documentación actualizada cuando un cambio importante de arquitectura o flujo cambia el contexto del proyecto.

## 4. Reglas de código

### 4.1. Mejoras mínimas y precisas
- Hacer cambios pequeños y directos.
- No reescribir áreas completas si no es necesario.
- No resolver problemas no relacionados con la tarea actual.

### 4.2. Mantener consistencia del proyecto
- Archivos, carpetas y nombres deben seguir este estilo:
  - PascalCase para clases, enums y métodos públicos,
  - camelCase para variables y parámetros,
  - carpetas con nombres descriptivos y en inglés,
  - `PortfolioAnalytics.` como prefijo del nombre del proyecto y subproyectos.

### 4.3. No sobre-documentar el código
- Comentarios solo cuando realicen una aclaración técnica valiosa.
- Preferir nombres expresivos a comentarios excesivos.

### 4.4. No dejar código provisional en producción
- No dejar `TODO` sin contexto serio.
- No dejar placeholders de negocio sin resolver.
- No dejar logs ruidosos ni debugging cruft.

### 4.5. Reglas de validación antes de cerrar una tarea
- Verificar compilación si aplica.
- Ejecutar la prueba más pequeña que cubra el cambio.
- Si el cambio afecta API, validar el flujo relevante.
- Si el cambio afecta negocio financiero, revisar la lógica de cálculo cuidadosamente.

## 5. Reglas específicas del dominio financiero

- Las métricas deben ser consistentes y reproducibles.
- Las series temporales deben normalizarse antes de cálculo.
- La deduplicación por símbolo + fecha + fuente debe ser tenida en cuenta.
- Los backtests deben ser reproducibles en función de los parámetros usados.
- Los cálculos financieros deben priorizar claridad sobre “magia matemática”.

## 6. Reglas para decisiones de implementación

### Si hay duda entre dos caminos:
- elegir la solución más simple,
- que resuelva el problema sin introducir complejidad innecesaria,
- y que permita avanzar al siguiente paso del MVP.

### Si una funcionalidad puede esperar:
- dejarla en el backlog como mejora futura,
- no bloquear el MVP con ella.

## 7. Reglas de comunicación en el proyecto

- Explicar el porqué antes del cómo.
- Mostrar consecuencias de la decisión arquitectónica.
- Explicar el impacto funcional de cada cambio.
- Mantener la documentación actualizada cuando cambie la arquitectura o el flujo.

## 8. Criterio final de buena implementación

Se considera una buena implementación si:
- resuelve un problema real del usuario,
- mantiene el código entendible,
- no introduce complejidad sin necesidad,
- está validada con pruebas o validación práctica,
- y encaja con el roadmap del MVP.

## 9. Excepción

Si el usuario pide explícitamente algo distinto, esa solicitud tiene prioridad sobre estas reglas.
