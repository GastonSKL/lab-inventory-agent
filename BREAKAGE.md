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

### 001 - Problema al cargar solución (2026-11-26, fase 1)

**Al cargar por primera vez la solucion después del scafolding, ambos proyectos salian como "Descargado", pero no se podía ver el contenido de los mismos
**Se corrió el MSBuild de Visual Studio a mano y dio Version 10.0.401 of the .NET SDK requires at least version 18.0.0 of MSBuild. The current available version of MSBuild is 17.14.
**Se arreglo simplemente volviendo a cargar el proyecto manualmente desde el mismo vs 2026
**Que por ahi algunos problemas se puedes resolver volviendo a cargar el proyecto 