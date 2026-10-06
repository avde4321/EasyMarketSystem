# Sprint 3 Robustecimiento SaaS - Migraciones idempotentes, rollback e índices de alto volumen

## Alcance implementado

Se agregó una migración incremental:

- `20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes`

La migración está enfocada en mejorar lecturas multiempresa y operaciones de alto volumen sin modificar datos transaccionales sensibles.

## Índices agregados o ajustados

### Facturación / Ventas

- `Facturas(EmpresaId, ClaveAcceso)` único.
- `Facturas(EmpresaId, Establecimiento, PuntoEmision, Secuencial)` único.
- `Facturas(EmpresaId, FechaEmision)`.
- `Facturas(EmpresaId, Estado)`.
- `Facturas(EmpresaId, Estado, CreatedAt)`.
- `Facturas(EmpresaId, Secuencial)`.
- `Facturas(EmpresaId, ClienteId)`.

Se reemplazaron índices globales anteriores sin `EmpresaId` para alinear el modelo SaaS multiempresa.

### Compras / Cuentas por Pagar

- `Compras(EmpresaId, ProveedorId, EstadoCompra)`.
- `Compras(EmpresaId, ProveedorId, FechaEmision)`.
- `Compras(EmpresaId, EstadoCompra, FechaEmision)`.
- `Compras(EmpresaId, ClaveAccesoProveedor)` filtrado cuando la clave no es nula.
- `CuentasPorPagar(EmpresaId, ProveedorId, EstadoDeuda)`.
- `CuentasPorPagar(EmpresaId, EstadoDeuda, FechaVence)`.

### Inventario / Kardex

Ya existía y se conservó:

- `KardexMovimientos(EmpresaId, BodegaId, ProductoId, FechaMovimiento)`.
- `KardexMovimientos(EmpresaId, FechaMovimiento, BodegaId, ProductoId)`.

No se duplicaron índices equivalentes.

### Tesorería / Conciliación Bancaria

Se agregaron campos denormalizados en `ExtractoBancarioDetalles`:

- `EmpresaId`.
- `CuentaBancariaId`.

Estos campos se rellenan automáticamente desde `ExtractoBancarioHeaders` y `CuentasBancarias` durante la migración.

Nuevos índices:

- `ExtractoBancarioDetalles(EmpresaId, CuentaBancariaId, FechaTransaccion)`.
- `ExtractoBancarioDetalles(EmpresaId, CuentaBancariaId, Conciliado, FechaTransaccion)`.
- `ExtractoBancarioDetalles(EmpresaId, CuentaBancariaId, NumeroDocumentoRef, Monto)`.

También se ajustaron consultas de tesorería/conciliación para filtrar por esos campos directos y reducir joins en consultas masivas.

### Documentos SRI / Outbox

- `ComprobanteCabecera(EmpresaId, ClaveAcceso)` único.
- `ComprobanteCabecera(EmpresaId, Estado)`.
- `ComprobanteCabecera(EmpresaId, Estado, CreatedAt)`.
- `ComprobanteCabecera(EmpresaId, FechaEmision)`.
- `ComprobantesRetencion(EmpresaId, ClaveAcceso)` único filtrado.
- `ComprobantesRetencion(EmpresaId, EstadoSRI)`.
- `ColaProcesamientoSRI(EmpresaId, ComprobanteId, TipoDocumentoId)`.
- `ColaProcesamientoSRI(EmpresaId, Estado)`.

Se conservó el índice existente:

- `ColaProcesamientoSRI(EmpresaId, Estado, NextRetryAt, CreatedAt)`.

## Scripts generados

Artefactos ubicados en `artifacts/database`:

- `TestDeIa_idempotent.sql`: script idempotente completo desde cero.
- `Sprint3_IndicesAltoVolumen_delta_idempotent.sql`: script idempotente delta recomendado para QA/Producción desde la migración anterior.
- `Sprint3_pre_migration_check.sql`: validaciones previas de duplicados y referencias requeridas.
- `Sprint3_rollback_safe.sql`: rollback operativo no destructivo orientado a revertir índices del sprint.

## Estrategia de despliegue recomendada

1. Ejecutar backup completo o snapshot.
2. Ejecutar `Sprint3_pre_migration_check.sql`.
3. Si el pre-check pasa, ejecutar `Sprint3_IndicesAltoVolumen_delta_idempotent.sql`.
4. Validar planes de consulta críticos:
   - dashboard ventas;
   - monitor SRI;
   - conciliación bancaria;
   - CxP por proveedor/estado;
   - búsquedas POS/facturación.

## Estrategia de rollback

Para producción se recomienda usar `Sprint3_rollback_safe.sql`.

Ese script:

- elimina los índices agregados en el sprint;
- restaura índices legacy necesarios;
- no elimina columnas ni datos transaccionales.

Las columnas nuevas de `ExtractoBancarioDetalles` se dejan en la tabla durante rollback operativo porque son denormalizadas, reconstruibles y no afectan el uso del modelo anterior si permanecen como columnas extra.

Para ambientes no productivos, si se requiere rollback EF estricto, puede usarse:

```powershell
dotnet ef database update 20260929040039_AddTesoreriaAndConciliacionBancaria --project TestDeIa.Infrastructure --startup-project TestDeIa.Api --context TestDeIaDbContext
```

## Validación técnica

- `dotnet build TestDeIa.sln --no-restore -v:minimal`: 0 errores, 0 warnings.
- `dotnet test TestDeIa.sln --no-build -v:minimal`: 7 pruebas pasadas.

## Riesgos y recomendaciones DBA

- Crear índices en tablas con millones de registros puede tomar tiempo y bloquear escrituras dependiendo de la edición de SQL Server. Programar ventana controlada.
- Si se usa SQL Server Enterprise, evaluar `ONLINE = ON` para índices grandes en un script DBA ajustado.
- Revisar fragmentación y estadísticas después del despliegue.
- Monitorear crecimiento de índices en `Facturas`, `KardexMovimientos`, `ExtractoBancarioDetalles` y `ComprobanteCabecera`.
- Mantener el script delta como artefacto principal de CI/CD para bases existentes; usar el script completo solo en fresh deploy controlado.
