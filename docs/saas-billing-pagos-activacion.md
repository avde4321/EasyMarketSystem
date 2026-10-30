# Fase 3 — Pagos SaaS y activación de suscripciones

## Implementado

Se agrega `ISaasBillingService` / `SaasBillingService` en las capas existentes, no un DbContext ni un sistema de pagos paralelo al ERP. Los pagos de suscripción son ingresos de la plataforma, no cobros de facturas de los clientes del ERP: se mantiene esa separación.

`SaasPagoRegistroEntity` se persiste en `SaasPagoRegistros`, con FK Guid a empresa, FK int a plan, monto decimal(18,4), fecha UTC, método/estado como enums persistidos en varchar, referencia hasta 200 caracteres y observaciones hasta 2000. Incluye usuario registrador, usuario/fecha/observación de decisión y rowversion. Filtro global por EmpresaId, índice por empresa/estado/fecha e índice único por empresa/método/referencia. No almacena imágenes ni credenciales bancarias.

### Registro de transferencia

`POST /api/v1/saas/billing/register-transfer`

Requiere JWT y permiso `empresa.configurar`. La empresa se obtiene del contexto autenticado, NO del body; se comprueba pertenencia y usuario activo en el servicio.

```json
{
  "saasPlanId": 1,
  "monto": 29.99,
  "comprobanteReferencia": "BANCO-DEPOSITO-123456",
  "observaciones": "Transferencia reportada por el cliente"
}
```

La referencia se normaliza en mayúsculas sin espacios. Si ya existe con el mismo plan/monto, devuelve el mismo pago y estado sin crear otro. Si cambia plan o monto, devuelve conflicto. El estado inicial siempre es Pendiente: aportar una referencia NO acredita dinero. El monto debe ser positivo, máximo cuatro decimales y coincidir con el precio mensual del plan activo. Los planes de precio personalizado/0 requieren cotización previa; no se aprueban gratuitamente desde este flujo. La fecha de pago registrada representa la fecha de presentación del comprobante; la fecha de verificación queda aparte.

### Confirmación administrativa

`POST /api/v1/saas/billing/confirm-payment`

```json
{
  "pagoId": "GUID_DEL_PAGO",
  "empresaId": "GUID_DE_LA_EMPRESA_DEL_PAGO",
  "aprobar": true,
  "observaciones": "Acreditación verificada en el banco"
}
```

JWT con rol reservado `SuperAdministrador`, además de verificación en BD del usuario/rol activo (endurecimiento posterior de instalación segura). Ni `Administrador` legado ni `AdministradorEmpresa` pueden aprobar pagos SaaS. El superadministrador puede indicar explícitamente la empresa objeto de revisión, sin exponer búsquedas abiertas a clientes. El pago se busca siempre por Id + EmpresaId. Ver `superadministrador-inicializacion.md` para crear la cuenta privada.

`AprobarPagoAsync` cambia pago a Aprobado, actualiza el plan seleccionado, estado Activa, inicio UTC actual y vencimiento `max(vencimientoActual, ahoraUTC).AddMonths(1)`. Respeta meses calendario y años bisiestos. Valida que haya una única suscripción y que el plan no quede por debajo de usuarios/puntos ya registrados. El precio queda congelado al registrar el pago; un cambio posterior de tarifa no altera un comprobante ya presentado.

`RechazarPagoAsync` solo cambia la decisión del pago: no activa ni modifica la suscripción. Repetir la misma decisión es idempotente; no se puede revertir Aprobado/Rechazado a otro estado mediante este endpoint. Devoluciones, contracargos y correcciones necesitan un proceso explícito futuro, no edición de pagos históricos.

Ambos endpoints están exceptuados del bloqueo por suscripción SOLO para POST y ruta exacta, conservando autenticación, permiso/rol, validación de empresa y rate limit. Así una empresa vencida puede reportar su renovación. Los demás endpoints permanecen protegidos.

## Atomicidad, concurrencia y cuotas

- Transacción explícita para cada operación con el mismo bloqueo SQL `SAAS-CUPO:{EmpresaId}` usado por emisión/reserva de facturas. Dos instancias se coordinan en la base, no con locks en memoria.
- Rowversion en el pago e índice único como defensas adicionales. Estrategia EF SQL Server con reintentos acotados (dos) para fallos transitorios/deadlocks, sin paquetes nuevos. La operación completa se reejecuta de forma idempotente.
- El pago pasa a Aprobado en la misma transacción que renueva y reinicia el contador. Si falla el guardado, también se revierte la auditoría.
- `FacturasEmitidasMesActual=0` y período del mes UTC actual al aprobar. Se agrega `SaasSuscripcion.FechaReinicioCupo` para que el historial de facturas no repueble inmediatamente el contador anterior.
- Las facturas confirmadas antes de ese corte dejan de consumir el nuevo cupo, pero NO se borran. Las reservas pendientes siguen consumiendo capacidad aunque se hayan creado antes del pago. Las nuevas autorizaciones se sincronizan usando mes UTC + fecha del corte. Las empresas sin corte conservan las reglas anteriores.
- Registrar el comprobante o repetir una aprobación NO reinicia cuotas. Solo la primera aprobación efectiva de un nuevo pago lo hace. Un pago anticipado extiende el vencimiento desde el actual y reinicia cupo inmediatamente, conforme al requerimiento; no se difiere el beneficio a la fecha futura.

## Auditoría y respuestas

Eventos nativos `PagoSaasRegistrado`, `PagoSaasAprobado`, `PagoSaasRechazado`; auditoría transaccional con empresa, actor, pago y plan. ILogger incluye EmpresaId y UsuarioId. La referencia del cliente y observaciones no se interpolan en logs de éxito.

Respuesta: `SaasPagoResponse` con identidad, empresa, plan, monto, fecha/método/estado, referencia y fecha de confirmación. ProblemDetails: 403 permisos, 404 pago/empresa no coincidentes, 409 reglas de negocio/conflictos de concurrencia. Errores de validación se manejan por ApiController.

## Cambios para DBA

Migración incremental `20261030033115_SaasBillingPagosYActivacion`:

1. Crea `SaasPagoRegistros` con FK restrict a empresa/plan, controles de monto/estado/método y los índices mencionados.
2. Agrega únicamente `SaasSuscripciones.FechaReinicioCupo` nullable datetimeoffset. El valor nulo mantiene compatibilidad con suscripciones existentes.
3. No elimina columnas, pagos ERP, asientos, cuotas históricas ni suscripciones.
4. Down bloqueado si existen pagos: exige preservar el historial financiero antes de retirar la tabla.

Migración aplicada en desarrollo. Revisar/importar en cada ambiente por el procedimiento habitual. La unicidad de referencias es por empresa/método: el revisor debe verificar importe, cuenta destino, titularidad y que una misma acreditación no se reclame por empresas diferentes. Un número aislado no es una prueba bancaria verificable automáticamente.

## Pruebas

Fechas vigentes/vencidas, fin de mes/año bisiesto, validación decimal bajo es-EC; transferencia pendiente, reintentos concurrentes de referencia y aprobación, una sola renovación, cuotas nuevas sin borrar historial, conteo posterior al reinicio, conservación de reservas, rechazo sin activación, denegación a administrador de empresa, aislamiento por empresa, monto incorrecto/plan inactivo y rollback de pago/suscripción/auditoría ante fallo de persistencia.

Los fixtures SQL se agrupan en una colección sin solapar su preparación/limpieza sobre catálogos y FKs compartidas. La concurrencia de negocio se conserva dentro de las pruebas mediante Task.WhenAll (ocho registros/aprobaciones simultáneas, además de las pruebas anteriores de cuotas). La limpieza elimina solamente sus empresas de prueba y admite reintento acotado si SQL la selecciona como víctima de deadlock; no se relajan las aserciones de negocio.

## No incluido / siguiente fase

- No hay un webhook anónimo, ni se aprueba una tarjeta basándose en datos enviados por el navegador. `TarjetaPasarela` queda preparado en el catálogo; falta seleccionar proveedor, verificar firma/origen, importe, moneda y evento antes de exponer un adaptador de webhook. No se reutilizan callbacks de cobros POS como si fueran pagos de suscripción.
- No hay carga de PDF/imagen en este contrato: se informa la referencia solicitada; los archivos deberán pasar por DocumentoAdjunto si se agregan.
- Falta pantalla de transferencias y revisión administrativa, integración real de pasarela, cotizaciones personalizadas, notificación de decisiones y política de reembolsos. No se han hecho cargos reales ni verificación bancaria automatizada.
