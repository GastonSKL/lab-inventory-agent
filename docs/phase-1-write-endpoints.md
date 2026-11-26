# Fase 1: endpoints de escritura (a cargo de Gastón)

Estos dos endpoints son los que el agente va a usar en la Fase 3 como herramientas de escritura.
Todas las reglas de abajo son **determinísticas** y viven en la API: el LLM nunca las valida.

## 1. Crear pedido de reposición

`POST /api/orders`

```json
{ "productId": 2, "quantity": 10, "requestedBy": "lab.analista1", "notes": "opcional" }
```

| Regla | Respuesta si no se cumple |
|---|---|
| `quantity` mayor a 0 y menor o igual a 10.000 | 400 ValidationProblem |
| `requestedBy` obligatorio, máximo 100 caracteres | 400 ValidationProblem |
| `notes` máximo 500 caracteres | 400 ValidationProblem |
| El producto existe | 404 |
| No hay otro pedido `Pending` para el mismo producto | 409 Conflict con ProblemDetails explicando cuál |

Si todo está bien: se crea en estado `Pending`, con `CreatedAtUtc` tomado de `TimeProvider`, y se responde
**201 Created** con `Location: /api/orders/{id}` y el `RestockOrderView` en el body.

## 2. Ajustar stock de un lote

`POST /api/lots/{lotId}/adjustments`

```json
{ "delta": -2, "reason": "Rotura de envase durante el traslado", "adjustedBy": "lab.analista1" }
```

| Regla | Respuesta si no se cumple |
|---|---|
| `delta` distinto de 0 | 400 ValidationProblem |
| `reason` obligatorio, entre 5 y 500 caracteres | 400 ValidationProblem |
| `adjustedBy` obligatorio, máximo 100 caracteres | 400 ValidationProblem |
| El lote existe | 404 |
| La cantidad resultante no queda negativa | 409 Conflict (decir cuánto hay) |
| No se suma stock (`delta > 0`) a un lote vencido | 409 Conflict |

Si todo está bien: en **un solo `SaveChangesAsync`** se actualiza `Lot.Quantity` y se inserta el
`StockAdjustment` de auditoría, así una cosa no puede pasar sin la otra. Responder **201 Created** con el
ajuste y la cantidad nueva del lote.

## Pistas

- Mirá cómo están hechos `ProductEndpoints` y `LotEndpoints`: `TypedResults`, `Results<...>` y `TimeProvider`.
- `AddValidation()` ya está activado: si el body es un `record` con atributos (`[Range]`, `[Required]`,
  `[StringLength]`), el 400 sale solo. Las reglas que necesitan la base (existe, duplicado, stock negativo) van
  en el handler.
- Para 409: `TypedResults.Problem(detail: "...", statusCode: StatusCodes.Status409Conflict)`.
- Ojo con `decimal` en SQLite: comparar o sumar `decimal` dentro de la consulta puede fallar. Hoy lo resolvemos
  trayendo los datos y operando en memoria.
- Escribí un test por cada fila de las tablas, en un archivo nuevo `WriteEndpointsTests.cs`.
  Los tests de escritura modifican la base, así que usá datos que no rompan los tests de lectura, o una factory
  propia para esa clase.

## Terminado cuando

- [ ] Los dos endpoints andan desde Scalar (`dotnet run` → http://localhost:5247/scalar).
- [ ] Un test por regla, todos en verde con `dotnet test --solution LabInventory.slnx`.
- [ ] Commit(s) en la rama `feat/phase-1-inventory-api`.
