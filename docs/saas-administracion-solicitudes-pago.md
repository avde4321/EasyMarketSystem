# Bandeja exclusiva del superadministrador: pagos y habilitación de planes

## Acceso y uso

- Usuario de plataforma existente: `AdminSaasSudo`, rol `SuperAdministrador`. No se modificaron su contraseña ni su correo y no se creó otro superusuario.
- Ruta Blazor: `/saas/solicitudes-pago`.
- Menú exclusivo: **Administración SaaS → Solicitudes de pago**. Tras un inicio de sesión exitoso, ese rol llega directamente a esta pantalla; los usuarios de empresa conservan su inicio anterior.
- Por defecto se muestran solicitudes pendientes de todas las empresas, incluidas las antiguas. Se puede filtrar por empresa/RUC/referencia, estado y fechas de registro, con páginas de 10/20/50 filas.
- Abrir **Revisar solicitud**, contrastar la referencia y el monto con la acreditación bancaria real. La aplicación no consulta al banco ni trata una referencia escrita como prueba de pago.
- Para aprobar, confirmar la verificación bancaria y pulsar **Confirmar aprobación y habilitar**. Para rechazar, indicar un motivo y confirmar el rechazo. Los registros ya decididos solo admiten consulta de detalle.
- La aprobación reutiliza el servicio transaccional existente: activa la suscripción, asigna el plan del pago, renueva un mes y reinicia el cupo según la política ya implementada. Un reintento no agrega otra renovación. Rechazar no modifica la suscripción.
- El portal del cliente refleja la nueva situación al actualizarse. No se agregó envío de email/WhatsApp ni pasarela bancaria.

## Backend y seguridad

- Nuevo GET `/api/v1/saas/billing/admin/payments`, protegido por `Authorize(Roles=SuperAdministrador)`.
- Se reutiliza POST `/api/v1/saas/billing/confirm-payment`; no se creó otro motor de aprobación.
- Tanto listar como decidir verifican en BD que el actor tenga el rol activo y sea un usuario activo/no bloqueado. Ni un administrador de empresa ni el rol heredado Administrador pueden acceder a la bandeja global.
- El acceso global con IgnoreQueryFilters es una excepción explícita de administración de plataforma, ejecutada solo después de esa verificación. No activa IsSystemContext ni cambia EmpresaId del contexto.
- Las escrituras conservan filtros explícitos por PagoId + EmpresaId, el bloqueo transaccional por empresa, RowVersion, estados definitivos y auditoría de actor/empresa.
- La bandeja y la confirmación están exentas únicamente de la validación comercial de suscripción del tenant seleccionado; no están exentas de autenticación/autorización. Un superusuario no necesita contratar un plan para revisar pagos de clientes.
- La UI oculta el menú y protege la ruta por rol. Esto es complementario a las defensas del backend, no su sustituto.

## Consulta y compatibilidad

- Lectura AsNoTracking, filtros SQL, Count antes de la proyección, orden estable FechaPago + Id y paginación de máximo 50.
- Fechas diarias interpretadas en UTC−5, comparando el intervalo UTC correspondiente con el registro almacenado. La fecha declarada del depósito se muestra aparte.
- Nuevos DTOs de consulta y lectura. Sin entidades ni migraciones nuevas. Se mantienen SaasPagoRegistros, SaasSuscripciones y SecurityAuditLogs.
- Se reutilizaron el diseño de tablas/popups, las notificaciones y ProcessingOverlay. Se bloquea la acción mientras procesa, se muestran errores ProblemDetails y se refresca la lista después de decidir.

## Verificación

- Compilación de solución completa: 0 errores y 0 advertencias.
- Pruebas SQL Server de lista global paginada, filtros por fecha ecuatoriana, acceso denegado a administradores normales y superusuario bloqueado, aprobación de otra empresa conservando el contexto, auditoría y reintento idempotente.
- Pruebas de contrato HTTP de filtros y EmpresaId/PagoId seleccionados; pruebas de autorización declarada y exención controlada del middleware de suscripciones.
- Suite completa: 172 aprobadas, 1 omitida (diagnóstico de certificado real opt-in, ajeno a este módulo), 0 fallidas.
- No se aprobaron ni rechazaron solicitudes reales del usuario. Las pruebas de decisión crean y limpian sus propios registros. La revisión visual interactiva en navegador no se ha certificado.

## Operación

Reiniciar API/cliente y volver a iniciar sesión con el usuario de plataforma para renovar los claims. No se deben otorgar este rol ni las credenciales a usuarios de empresas. Si el volumen global de pagos crece considerablemente, evaluar índices EstadoPago + FechaPago + Id; la consulta ya limita el tamaño de cada respuesta.
