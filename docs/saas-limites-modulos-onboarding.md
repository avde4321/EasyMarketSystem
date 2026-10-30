# SaaS: límites, módulos y alta inicial de empresa

## Implementado

### Usuarios y puntos de emisión

- Creación de usuarios mediante `SecurityManagementUseCase` y `EfSecurityUserRepository`: bloqueo transaccional por empresa y `CanCrearUsuarioAsync` antes de asignar el nuevo asiento.
- La transacción de aplicación abarca persona y usuario: un rechazo por cuota no deja una persona nueva huérfana. También cubre edición cuando modifica accesos.
- Agregar acceso de un usuario existente a otra empresa comprueba la capacidad de esa empresa. Los bloqueos de varias empresas se toman ordenados.
- `EfEmpresaRepository.SaveAsync` verifica `CanCrearSucursalAsync` por cada punto nuevo, bajo el mismo bloqueo de empresa. Considera las altas y bajas aún pendientes en ChangeTracker; un lote que supera el plan revierte completo.
- Se conservan las definiciones existentes: un asiento es una asignación usuario–empresa, incluso si el usuario está inactivo; el administrador propietario consume un asiento. La cuota de sucursales cuenta puntos de emisión, no códigos de establecimiento distintos. Cero significa ilimitado.
- Los controladores relevantes dejan pasar `SaaSQuotaExceededException` al middleware para devolver HTTP 403 con ProblemDetails, en lugar de transformarla en HTTP 409. Los clientes de Empresa y Usuarios muestran el detalle recibido.

### Permisos por plan

`EnsureModuleAsync` y `RequirePlanModuleAttribute` verifican suscripción vigente y banderas del plan en base de datos, sin confiar en claims comerciales potencialmente desactualizados. Conservan las políticas de rol existentes: tener el módulo no concede permisos de usuario.

- SRI: retenciones, catálogo SRI, importación XML SRI, configuración de secuenciales, emisión/monitor/reproceso/notas de crédito, conversión de proforma a factura, sincronización offline y reportes de documentos electrónicos.
- Defensa adicional en creación de factura, nota de crédito, retención y liquidación: las llamadas internas tampoco pueden crear comprobantes nuevos sin el módulo SRI.
- Kárdex avanzado: historia de movimientos por producto y reporte de kárdex valorizado.
- No se bloquean por esa bandera productos, consulta de stock, bodegas ni los movimientos internos necesarios para operar el ERP/POS. Tampoco se detiene el procesamiento de facturas ya comprometidas por cambiar posteriormente de plan.
- La vigencia se verifica con fecha y hora UTC exactas; una suscripción futura, vencida, suspendida, cancelada o un plan inactivo no habilita módulos.

### Onboarding

- Nuevo `ITenantOnboardingService` / `TenantOnboardingService`, integrado dentro de la transacción existente de creación de empresa.
- Reutiliza al administrador/propietario autenticado y comprueba que exista, esté activo y tenga acceso a la empresa. No duplica su usuario ni le eleva roles globales.
- Asigna una suscripción inicial `Activa` por **15 días**. El plan inicial puede seleccionarse en Nueva empresa; si se omite, se usa `Saas:DemoPlanId` (predeterminado 1).
- La elección del plan corresponde al período de prueba: no significa pago recibido ni activación comercial indefinida. Se registra el evento `SuscripcionInicial` con esa distinción en auditoría.
- Repetir el alta o editar la empresa no renueva el período ni crea otra suscripción. Un plan inexistente/inactivo o una configuración excesiva revierte el alta completa, incluidos accesos y suscripción.
- Los antiguos tenants sin suscripción no reciben pruebas nuevas automáticamente al editarlos: su regularización sigue siendo administrativa.

## Qué se conservó y alcance

Arquitectura Application/Ports/Infrastructure, `TestDeIaDbContext`, EmpresaId Guid, tablas y relaciones actuales. Se agrega una abstracción transaccional pequeña para los casos de uso sin introducir EF Core en Application. No hay nuevos paquetes ni cambios de esquema; EF confirma que no existen cambios pendientes de modelo.

El onboarding implementado es el de **Nueva empresa con administrador autenticado**, que es el flujo existente del proyecto. No se abrió un endpoint anónimo de registro público. Un alta pública completa necesitaría verificación de identidad/correo, controles de abuso y una política de identidad global antes de habilitarla.

## Validación

- 74 pruebas aprobadas, sin omitidas al configurar `EASYMARKET_TEST_SQL`.
- SQL Server: 12 altas simultáneas con un asiento disponible → una aceptada; los rechazos revierten sus personas.
- SQL Server: 12 altas simultáneas con un punto disponible → una aceptada.
- SQL Server: un lote de dos puntos en plan de uno no guarda ninguno.
- SQL Server: empresa nueva recibe prueba de 15 días; un plan inválido no deja empresa ni asignación de usuario; reintentar no duplica la suscripción.
- Filtro: HTTP 403 para módulo ausente, incluso con rol Administrador; permisos efectivos cambian al modificar el plan sin renovar JWT.
- Pruebas de vigencia, plan inactivo, propietario incorrecto y adiciones de lote sin guardar.
- No se enviaron documentos al SRI ni se iniciaron servicios externos para estas pruebas.

## Pendientes fuera de esta entrega

Gestión comercial de suscripciones (pago, renovación, cambio de plan y suspensión), regularización de empresas antiguas sin plan, consolidación de selección inequívoca cuando existan varias suscripciones históricas y experiencia visual de upgrade/ocultación de opciones según capacidades. El backend ya devuelve las restricciones aunque las opciones permanezcan visibles.

Para producción: revisar el plan predeterminado, cupos incluidos y política de pruebas de cada tier; configurar `Saas__DemoPlanId` si corresponde. Desplegar API y cliente juntos. No se requiere nueva migración por esta entrega.
