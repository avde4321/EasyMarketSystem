# Cuotas SaaS de facturación: reserva y confirmación atómicas

## Cambios

- Se elimina el incremento desde `MarkFacturaAsReceivedAsync`: recibir o reintentar una factura no aumenta `FacturasEmitidasMesActual`.
- `SaasFacturaCupos` registra un único cupo por `FacturaId`, con `EmpresaId` Guid y estados RESERVADO / CONFIRMADO / LIBERADO.
- La creación de factura reserva capacidad dentro de su misma transacción, **antes del secuencial**. Si falla cualquier paso, el rollback revierte también la reserva. La FK documental no se agrega porque la reserva precede al INSERT de factura; existe FK de empresa y la atomicidad evita reservas huérfanas en el flujo normal.
- El cupo se confirma cuando la factura ya está persistida como AUTORIZADO, dentro de la misma transacción que su autorización. Se integran tanto el worker SRI como la autorización inmediata en modo desarrollo.
- Repetir una autorización no duplica el consumo ni el evento de autorización. No se bloquea el registro de una respuesta SRI ya autorizada por un vencimiento posterior del plan: se registra la verdad del comprobante y su consumo.
- Los rechazos liberan reservas, pero no descuentan consumos ya confirmados. Una factura con error de red o pendiente de autorización conserva el cupo reservado.
- El worker comprueba/reutiliza la reserva al recibir el documento; no abre una transacción SQL durante la comunicación HTTP con el SRI.

## Concurrencia

SQL Server `sp_getapplock` exclusivo por empresa, con dueño Transaction y timeout de 15 segundos. La reserva, lectura de capacidad, actualización de consumo y autorización siguen el mismo orden de bloqueo. Empresas distintas usan recursos de bloqueo distintos; no se usan locks en memoria para producción.

Disponibilidad = límite mensual - autorizadas del período UTC actual - reservas pendientes, incluidas las de meses anteriores. El cero del límite conserva su significado de ilimitado. Las reservas no incrementan el contador de autorizadas.

El índice único primario por factura garantiza la identidad del consumo. El índice `(EmpresaId, Estado, PeriodoAutorizacion)` sirve a las consultas de capacidad. La confirmación recalcula el snapshot mensual desde el registro de consumos bajo el bloqueo transaccional, evitando incrementos perdidos. El período utiliza UTC, siguiendo el comportamiento previo del módulo SaaS.

No se borran reservas por tiempo automáticamente: una factura puede estar en contingencia o en procesamiento SRI. Debe resolverse su estado real antes de liberar capacidad. Cambiar de plan no reinicia el historial de consumo por empresa.

## Migración y DBA

`20261029040210_SaasCuposFacturaAtomicos`, aplicada en la base local de desarrollo `TestDeIaCrm`.

La migración es aditiva y reconstruye el registro usando las facturas existentes: autorizadas como confirmadas; NO_FIRMADO/PENDIENTE como reservadas. Corrige los snapshots mensuales que antes podían contar intentos fallidos. Para autorizadas históricas sin fecha de autorización se usa CreatedAt como respaldo; revisar esa anomalía en otros entornos.

Para despliegue productivo: respaldo previo y detener todas las instancias antiguas antes de migrar. No mezclar workers con el algoritmo anterior y el nuevo. El rollback se bloquea si ya existen reservas/consumos, para no perder el registro de idempotencia. Si las reservas históricas superan un plan, se bloquean nuevas emisiones hasta resolverlas; no se descartan automáticamente.

No se cambian límites de usuarios/sucursales, onboarding, selección de suscripción vigente ni permisos comerciales por módulo: siguen siendo pendientes separados de esta entrega. Ante múltiples suscripciones, la selección del plan sigue la lógica existente; su consolidación permanece pendiente.

## Validación

- Suite ejecutada con SQL Server: 61 pruebas aprobadas, sin omitidas.
- 20 terminales simultáneas, límite 1: exactamente una reserva aceptada; contador de autorizadas permanece en 0.
- 12 callbacks concurrentes mediante el repositorio de facturación: un solo consumo confirmado y un solo evento de autorización.
- Rollback de creación: no queda reserva ni se pierde capacidad.
- Pruebas adicionales: reintento idempotente, liberación por rechazo, bloqueo sin autorización, reserva de otro tenant y pendientes de meses anteriores.
- Los tests SQL usan empresas/planes con identificadores exclusivos y eliminan únicamente sus propios datos al terminar. No llaman al SRI.

Para repetir las pruebas SQL, configurar `EASYMARKET_TEST_SQL` con una base de pruebas migrada y ejecutar `dotnet test TestDeIa.Tests/TestDeIa.Tests.csproj`. Sin esa variable se omiten explícitamente las tres pruebas SQL; las pruebas InMemory solo validan reglas, no concurrencia relacional.
