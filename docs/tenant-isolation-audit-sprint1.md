# Sprint 1 Robustecimiento SaaS - Auditoría de aislamiento multiempresa

Fecha: 2026-09-29

## Alcance ejecutado

- Se auditó el uso de `IgnoreQueryFilters()` en API, infraestructura, workers, servicios SRI, integraciones, compras, facturación, POS offline y seeding.
- Se agregó la interfaz `ITenantEntity` en la capa de persistencia para marcar explícitamente las entidades con `Guid EmpresaId`.
- Se marcaron las entidades de persistencia que tienen `EmpresaId` directo.
- Se agregó filtro global faltante para `SecurityUserPuntoEmisionEntity`.
- Se creó el proyecto `TestDeIa.Tests`.
- Se agregó una suite automatizada de aislamiento multiempresa.

## Hallazgo corregido

La entidad `SecurityUserPuntoEmisionEntity` tenía `EmpresaId`, pero no contaba con filtro global activo en `TestDeIaDbContext`.

Corrección aplicada:

- `SecurityUserPuntoEmisionEntity` implementa `ITenantEntity`.
- `TestDeIaDbContext` ahora aplica filtro global por `EmpresaId`.

## Clasificación de `IgnoreQueryFilters()`

Los usos encontrados quedan permitidos únicamente en estos escenarios:

- Inicialización/seeding controlado del sistema.
- Workers o procesadores background que toman registros por identificador y estado controlado.
- Procesos SRI/Outbox que revalidan `EmpresaId` desde el registro tomado.
- Validaciones de seguridad/login donde aún no existe tenant activo.
- Servicios de diagnóstico/prueba que ejecutan escenarios controlados.

Regla de mantenimiento:

- Todo nuevo uso de `IgnoreQueryFilters()` debe justificar el bypass y revalidar manualmente el `EmpresaId` cuando lea o modifique información transaccional o maestra de una empresa.

## Pruebas agregadas

Proyecto:

- `TestDeIa.Tests`

Suite:

- `TenantIsolationTests`

Cobertura:

- Entidades con `Guid EmpresaId` deben implementar `ITenantEntity`.
- Entidades `ITenantEntity` deben tener filtro global de EF Core.
- Empresa A no puede leer registros `Persona` de Empresa B.
- Contexto de sistema puede leer todos los tenants para workers/seeding controlados.

## Resultado

- `dotnet test TestDeIa.sln --no-restore -v:minimal`: aprobado.
- Pruebas ejecutadas: 4.
- Errores: 0.
