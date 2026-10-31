# Fase 4 — Portal de renovación Blazor

## Interfaz

`/subscription-expired` ahora es un portal de renovación dentro del layout existente, con tarjetas dinámicas de los planes activos, precio mensual USD, límites de facturas/usuarios/sucursales y módulos SRI/Kardex. La selección es explícita. Planes con importe 0 requieren cotización: no se ofrece un cobro gratuito.

El administrador de empresa puede abrir el modal estándar de transferencia: referencia obligatoria (hasta 200 caracteres), fecha obligatoria y observación opcional (hasta 2000). El precio se toma del plan seleccionado; el backend continúa verificándolo. Se reutilizan `ProcessingOverlay`, toasts, estilos, botones, sesión y permisos existentes. Hay cierre por botón/Escape, foco inicial en el diálogo, errores inline y bloqueo de campos/acciones durante el POST. El modal no solicita tarjetas, credenciales ni archivos binarios. No se inventan datos de una cuenta bancaria: el texto exige solicitar las instrucciones bancarias al equipo administrativo antes de transferir.

Tras el registro, el modal cierra y se muestra el estado pendiente con referencia, monto, plan y fecha. Los controles de nuevo pago quedan deshabilitados mientras existe una solicitud pendiente. «Actualizar estado» vuelve a consultar; al recargar la página se conserva el pendiente porque se lee de SQL y no de una variable local. Si el pago ya se aprobó, el portal indica servicio activo y ofrece acceso al dashboard. La salida «Cerrar sesión / cambiar de cuenta» usa el logout existente, que limpia token y empresa activa.

La consulta de estado se permite a miembros autenticados de la empresa; registrar pagos requiere `empresa.configurar`. Usuarios sin ese permiso ven el estado y una indicación para contactar al administrador. Si no se logra consultar el estado, se bloquean acciones de pago para evitar registros a ciegas.

## APIs y persistencia

- `GET /api/v1/saas/billing/portal`: resumen de suscripción, último pago y hasta 10 pendientes de la empresa autenticada. Solo lectura, AsNoTracking y proyección SQL. No permite elegir otra empresa por query/body ni desactivar filtros sin validación explícita de pertenencia.
- `POST /api/v1/saas/billing/register-transfer`: contrato existente ampliado con `FechaPago` opcional DateOnly para compatibilidad con consumidores de la Fase 3. La UI sí exige ese dato. Rechaza fechas futuras usando fecha continental de Ecuador (UTC-5). La información se persiste como `FechaPagoInformada` y no reemplaza la fecha UTC de registro/auditoría existente.
- `POST /api/v1/saas/billing/checkout`: consulta del flujo de tarjeta al backend. Valida pertenencia y plan activo, devuelve `Disponible=false` con explicación. No crea pagos ni aprueba suscripciones: NO hay una pasarela SaaS contratada/configurada. El botón lo informa claramente; no simula un cobro exitoso.
- Se excluye del bloqueo por suscripción el GET exacto de portal; registro y checkout conservan sus excepciones de renovación, autenticación y permisos. La aprobación queda reservada al rol `SuperAdministrador` mediante la inicialización segura posterior; el `Administrador` legado no puede aprobar.

Cliente HTTP dedicado `SaasBillingApiClient`, usando el cliente autenticado existente. Maneja ProblemDetails, validaciones, HTTP 403/429 y respuestas no JSON; no expone errores internos 5xx. No reintenta automáticamente transferencias. Un error de red aconseja consultar el estado antes de repetir la referencia; el backend conserva su idempotencia y protección de concurrencia.

## Migración / DBA

`20261030034928_SaasPortalFechaTransferencia`: agrega únicamente `SaasPagoRegistros.FechaPagoInformada` tipo date nullable. No altera las fechas históricas ni elimina columnas. Aplicada en desarrollo. El rollback se bloquea si ya existen fechas informadas, para evitar pérdida silenciosa de información.

No se cambian la fórmula de renovación mensual, la aprobación idempotente, el corte de cuotas, las reservas pendientes ni el modelo multiempresa de la Fase 3.

## Verificación realizada

- Compilación completa sin errores ni advertencias.
- Suite de 112 pruebas sin fallos ni omisiones: incluye aislamiento del resumen, persistencia de fecha, rechazo de fecha futura, checkout sin efecto financiero, acceso al portal con suscripción expirada y errores HTTP del cliente.
- Navegador: cliente Blazor real con backend QA aislado en memoria, sin SQL ni cargos reales. Verificados selección de plan, aviso de tarjeta no disponible, modal, validación sin referencia, animación/campos deshabilitados durante POST, confirmación/toast, pendiente tras recarga y bloqueo de nueva solicitud.
- Comprobado escritorio 1280×900 y móvil 390×844 sin desbordamiento horizontal del portal. Se reinició el servidor de cliente para verificar el bundle CSS actualizado.
- Evidencia: `docs/qa/portal-renovacion.jpg`.

## QA reproducible y límites

`TestDeIa.Tests/Ui/renewal-preview.mjs` es un proxy de revisión local, vinculado solo a 127.0.0.1, con datos y token sintéticos. Ejecutar el cliente en 127.0.0.1:5177 y el script con Node para abrir 127.0.0.1:5178/subscription-expired. No se incluye en el cliente publicado y nunca debe exponerse a la red ni usarse contra un backend real. Las pruebas SQL automatizadas verifican por separado la persistencia y reglas reales. No se iniciaron workers SRI durante esta revisión.

No se incluyen cobro real de tarjeta, cuenta bancaria receptora configurable ni envío automático de email/WhatsApp al aprobar. Se conserva el mensaje de revisión solicitado; en esta fase la comprobación efectiva se realiza con «Actualizar estado». La próxima integración deberá definir el canal real de notificación y el proveedor de pagos antes de prometer entrega automática.
