# Fase 1 — Auto-registro público de empresas

## Contrato y flujo

`POST /api/v1/saas/public/register`, anónimo, con política por IP de 5 solicitudes por hora (y limitador global existente). Recibe `PublicTenantRegisterDto` y devuelve el `LoginResponse` existente. El nombre de usuario es el email; no se devuelve ni registra la contraseña. RUC: exactamente 13 dígitos ASCII. Contraseña: 12–128 caracteres, mayúscula, minúscula, número y símbolo.

`ITenantOnboardingService.RegistrarPublicoAsync` implementa una transacción explícita de EF Core, compatible con su estrategia de ejecución. Valida email/usuario globales, RUC global y plan activo. Guarda empresa, persona propietaria, usuario, pertenencia, rol, punto, acceso al punto, suscripción, secuenciales, catálogo NIIF y auditoría. Genera el JWT antes del commit: si falla cualquier paso, se revierte TODO. Los índices únicos evitan altas duplicadas concurrentes. Los conflictos se presentan como ProblemDetails 409 genérico, sin revelar datos de otra empresa.

### Suscripción

- `EsDemo=true`: `Demo`, inicio UTC y vencimiento exactamente 15 días después.
- `EsDemo=false`: `PendientePago`, vencimiento 3 días después. NO implica pago confirmado. Los guardas existentes solo habilitan `Activa`/`Demo`; por tanto bloquean las operaciones de esta suscripción hasta una confirmación de pago posterior.
- Se conserva el flujo autenticado previo `AsignarSuscripcionInicialAsync`; no se cambia su comportamiento ni se renuevan suscripciones existentes desde este endpoint.

## Adaptaciones a la arquitectura existente

- No se crea `AspNetUsers` ni un segundo sistema de identidad: se utiliza `SecurityUsers`, el servicio existente de hash PBKDF2 de ASP.NET Core Identity y el generador JWT existente.
- Se agrega `AdministradorEmpresa`, con permisos ERP/POS pero sin `catalogos.administrar`, que protege catálogos globales. Los endpoints/pantallas de administración de usuarios y comisiones admiten ambos administradores. Se bloquea que un administrador de empresa asigne roles con permisos de mantenimiento global (incluido el `Gerente` legado). El administrador global existente no cambia.
- Como el contrato no solicita identificación fiscal personal, se crea una identificación interna provisional `REG...`, tipo interno `00`. No se usa el RUC de la empresa como cédula del propietario ni se crea al propietario como cliente/proveedor. Debe completar su identificación fiscal antes de utilizar esa persona en documentos tributarios.
- Empresa en ambiente SRI `1` (pruebas), emisión normal `1`, sin simulación automática de autorización, sin certificado precargado.
- Punto `001-001`, con `NombreCaja="Caja General"`. La arquitectura actual no tiene catálogo independiente de cajas; la caja operativa se identifica por punto de emisión. NO se crea una `CajaSesion` ficticia ni se registra efectivo. Se reutiliza además la cuenta contable NIIF Caja General.
- Bodega nula: el cliente debe crear/asignar su bodega y abrir la caja antes de operar el POS.
- Contadores de documentos `01`, `03`, `04`, `07`: `UltimoSecuencial=0`, por lo que la primera reserva genera `000000001`. Solo se inicializa ambiente pruebas. No se envían documentos al SRI durante el registro.

## Cambios exactos para DBA

Migración: `20261029044049_PublicTenantOnboarding`.

1. `SecurityUsers.UserName` y `NormalizedUserName`: se amplían de nvarchar(80) a nvarchar(180) para permitir email como login.
2. Índices únicos globales de `NormalizedEmail` y `NormalizedUserName`, manteniendo los índices multiempresa actuales. El login existente busca globalmente antes de seleccionar empresa; permitir duplicados globales produciría cuentas ambiguas. La migración comprueba duplicados y aborta con mensaje explícito si existen: nunca elimina ni fusiona usuarios.
3. `EmpresaPuntosEmision.NombreCaja`: nvarchar(100), requerido, default `Caja General`. No se cambian IDs ni relaciones de caja existentes.
4. Semilla del rol `AdministradorEmpresa` y sus permisos.
5. Nuevo valor de dominio `PendientePago`, persistido por la conversión string existente en `SaasSuscripciones.EstadoSuscripcion`. No requiere columna nueva.
6. Ninguna tabla paralela de empresa, usuarios, cajas o suscripciones. No se eliminan columnas ni registros transaccionales.

El `Down` se bloquea si hay usuarios que dependen del nuevo rol o nombres de más de 80 caracteres. En ese caso se requiere una estrategia de compatibilidad/backup antes del rollback. La baja de esquema no debe utilizarse como mecanismo de borrado de clientes.

## Verificación

Pruebas de validación del RUC y permisos del rol; integración real SQL Server para alta demo, alta pendiente de pago, reversión de empresa/usuario/suscripción/puntos/secuenciales/contabilidad/auditoría al fallar JWT, rechazo de RUC/email duplicados y cuatro solicitudes concurrentes con una única alta exitosa. Se verifica que no se creen bodegas ni sesiones de caja, que la contraseña tenga hash verificable y que el usuario pueda resolverse mediante el login existente.

Ejecutar las pruebas SQL configurando `EASYMARKET_TEST_SQL` con una conexión de pruebas. Los fixtures crean datos exclusivos y eliminan únicamente sus registros al finalizar; no ejecutar pruebas sobre producción.

## Alcance pendiente de fases posteriores

Verificación de email/teléfono y titularidad, CAPTCHA/prevención distribuida de abuso, recuperación de alta si se pierde la respuesta y confirmación real de pagos/webhook. El formulario público y selector de planes se incorporaron en la Fase 2 (ver `saas-registro-publico-blazor.md`). El límite por IP es en memoria por instancia; un despliegue multiinstancia debe añadir control en el perímetro. El estado pendiente no se activa automáticamente por llegar su fecha límite. El endpoint no permite registrar otro RUC o email existente ni asociarse unilateralmente a una empresa ya registrada.
