# BREAKAGE

Registro de qué se rompió durante la construcción, en orden cronológico: qué pasó, por qué y cómo se arregló.
Solo entradas reales.

## Formato de cada entrada

```
### NNN — Título corto (AAAA-MM-DD, fase N)

**Qué pasó:** el síntoma observable (error, respuesta incorrecta, comportamiento inesperado).
**Por qué:** la causa raíz, confirmada con un diagnóstico, no supuesta.
**Cómo se arregló:** el cambio concreto (commit).
**Qué aprendí:** lo que cambiaría en el diseño o en el proceso.
```

---

### 001 — Visual Studio 2022 no carga proyectos .NET 10 (2026-11-25, fase 1)

**Qué pasó:** al abrir por primera vez la solución después del scaffolding, en Visual Studio 2022 (17.14) los dos
proyectos aparecían como "Descargado" y no se podía ver su contenido. Desde la terminal, en cambio,
`dotnet build` y `dotnet test` funcionaban bien.

**Por qué:** Visual Studio no usa el MSBuild del SDK de .NET, sino el suyo propio. Al correr el MSBuild de
Visual Studio a mano sobre el proyecto, el error fue:
`Version 10.0.401 of the .NET SDK requires at least version 18.0.0 of MSBuild. The current available version of MSBuild is 17.14.40.60911.`
MSBuild 18 viene recién con Visual Studio 2026.

**Cómo se arregló:**
1. Se instaló Visual Studio 2026 Community (18.12.1) con winget, junto al 2022.
2. El primer intento de instalación falló con el código 8006. El log del instalador decía
   `Pre-check verification failed: VSProcessesRunning`: Visual Studio 2022 seguía abierto. Se cerró y se reintentó.
3. Ya en Visual Studio 2026 los proyectos seguían como "Descargado": el estado "descargado" que guardó
   Visual Studio 2022 en `.vs/` se heredó. Se resolvió con "Volver a cargar el proyecto".

Sin cambios en el código: la solución compila con el MSBuild de Visual Studio 2026 (18.12.3).

**Qué aprendí:**
- Que la terminal compile no garantiza que el IDE pueda cargar el proyecto: usan MSBuilds distintos.
- Correr la herramienta a mano para obtener el error exacto es más rápido que reinstalar a ciegas.
- Algunos problemas del IDE son estado guardado en caché (`.vs/`) y se resuelven recargando el proyecto,
  pero conviene confirmar primero que la causa de fondo ya está arreglada.
