# ADR-0001: Stack inicial de solución

- **Estado:** Aceptada
- **Fecha:** 2026-11-26

## Contexto

¿Que necesitaba el proyecto? Una API que corra fácil al clonarla, que sea testeable y que tenga soporte largo.

## Opciones consideradas

1. **Opción A**: .NET 10 LTS, SQLite, EnsureCreated, TimeProvider inyectado, MinimalApi
2. **Opción B**: .NET 11 (STS), SQL Server, Migrations de EF, DateTime.UtcNow directo, Controllers

## Decisión
.NET 10 LTS: .NET 10: el stack (Docker images, agentes de Azure DevOps, librerías de terceros, providers) ya está probado, y no necesitás nada de EF 11.
SQLite: Alguien que clona el repo gana arrancar en minutos, sin instalar nada.
EnsureCreated: EnsureCreated te da una base lista con una línea de código.
TimeProvider inyectado: Los tests dejan de depender de la hora real.
MinimalApi: Porque la API es chica

## Consecuencias

El riesgo más grande es la combinación SQLite + EnsureCreated. Cuando se quiera pasar a algo en produccion con data real, hay que hacer dos migraciones a la vez, la de motor y de manejo
de esquema. Mientras mas tarde se haga mas va a doler a la larga.

