# Superadministrador privado de plataforma

## Creación inicial controlada

Desde la raíz del proyecto, ejecutar en PowerShell:

```powershell
.\scripts\Initialize-SaasSuperAdmin.ps1
```

Usuario predeterminado: `AdminSaasSudo`. El script pide TU correo y una contraseña con confirmación oculta. No publiques la contraseña en el chat ni la escribas en un archivo o comando. Requisitos: 16–128 caracteres, mayúscula, minúscula, número y símbolo, sin espacios. Se puede elegir otro usuario con `-UserName` y un nombre con `-NombreCompleto`.

El entorno por defecto es Development (incluye User Secrets del proyecto API). Para un despliegue usar `-Environment Production` y configurar previamente `ConnectionStrings__DefaultConnection` mediante el mecanismo seguro del entorno. `-EmpresaId` permite elegir explícitamente la empresa de instalación; por defecto usa la empresa semilla existente `22222222-2222-2222-2222-222222222222`. Esa vinculación técnica no cambia el propietario de empresas de clientes ni otorga acceso irrestricto a sus datos ERP.

El script invoca el modo local `--bootstrap-superadmin` de la API. Ese modo aplica migraciones y ejecuta la inicialización; termina SIN abrir puertos, iniciar Kestrel, enviar documentos SRI ni arrancar los workers. No exige una clave JWT solo para crear la cuenta, porque no se emiten tokens durante la instalación. El inicio normal de la API conserva sus requisitos JWT.

La contraseña se pasa al proceso hijo por variable de entorno temporal, no por argumentos, SQL o logs, y se limpia/restaura al terminar. El hash se genera con el servicio existente (PasswordHasher de ASP.NET Core Identity/PBKDF2). El script no conserva ni imprime la contraseña. Mantener acceso exclusivo al host y al gestor de secretos: otros administradores del sistema operativo/SQL con privilegios totales siguen siendo actores confiables de la instalación.

## Inicialización automática en primer arranque

Para CI/CD o primera ejecución sin interacción, suministrar desde un gestor de secretos/variables de entorno:

```text
Saas__SuperAdminBootstrap__Enabled=true
Saas__SuperAdminBootstrap__UserName=tu_usuario_privado
Saas__SuperAdminBootstrap__Email=tu_correo
Saas__SuperAdminBootstrap__Password=(secreto del entorno)
Saas__SuperAdminBootstrap__NombreCompleto=(nombre del propietario)
Saas__SuperAdminBootstrap__EmpresaId=(opcional, Guid de empresa de instalación)
```

La API ejecuta este paso tras la inicialización de esquema y antes de escuchar peticiones. Por defecto `Enabled=false`: nunca se inventan credenciales. Si se habilita el bootstrap y faltan datos, se detiene el arranque en vez de crear una cuenta con clave conocida. Después de la creación, retirar la contraseña del entorno y deshabilitar el bootstrap.

## Idempotencia y seguridad

- Rol reservado `SuperAdministrador`; nunca se promueve automáticamente al `admin` de desarrollo.
- Creación atómica de persona interna, usuario, hash, rol, pertenencia a empresa, marca de instalación y auditoría. Sin cédula/RUC personal inventados: identidad interna de sistema `00 / ROOT-...`.
- Transacción explícita, bloqueo SQL global de instalación y PK única `SaasInicializaciones.Clave='SUPERADMIN'`.
- Al existir la marca, no se cambia contraseña, estado, bloqueo ni roles; tampoco se recrea la cuenta si fue retirada manualmente. Una asignación manual del rol sin marca genera bloqueo y revisión, no una segunda cuenta.
- Usuario/correo ya existente produce error. Nunca se eleva una cuenta encontrada por coincidencia de email.
- Rol excluido de `SecurityRoleNames.All` y del catálogo de asignación de usuarios. El repositorio rechaza explícitamente asignarlo incluso si una llamada interna omite la validación de Application.
- La cuenta queda fuera de listados normales de usuarios; se permite su resolución global para iniciar sesión. Los endpoints de empresa no pueden editarla, bloquearla, cambiar sus roles, desbloquearla ni restablecer su contraseña. Su recuperación/rotación futura exige un procedimiento de plataforma, no las pantallas de clientes.
- Aprobación/rechazo de pagos: tanto atributo API como validación contra BD exigen SuperAdministrador activo. AdministradorEmpresa, Administrador y Gerente NO obtienen ese derecho por mantener el rol legado.
- El rol también satisface las pantallas operativas que admiten administradores, pero no se desactivan los controles de pertenencia, suscripción o módulos del ERP.

No se implementó MFA en esta tarea. Sigue siendo una prioridad antes de publicar las operaciones privilegiadas. Tampoco se ha creado todavía la bandeja de revisión de pagos; el endpoint protegido existente queda reservado a esta nueva cuenta.

### Retiro explícito de un cajero anterior del mismo propietario

Si el correo personal está ocupado por una cuenta de pruebas con rol exclusivamente Cajero, el propietario puede autorizar su retiro en el modo local pasando `-RetirarCajeroId GUID` al script. No se aplica automáticamente por coincidencia de email: requiere el identificador exacto, modo forzado de instalación local, mismo correo y un único rol Cajero. No admite retirar administradores ni otros perfiles.

El retiro y la creación del superadministrador pertenecen a una sola transacción: si falla el alta, se revierte también la baja. Se conserva el Guid anterior para trazabilidad de cajas/facturas/auditoría; se desactiva y bloquea la cuenta, revoca sesiones, quita roles, pertenencias y asignaciones de punto, sustituye su credencial y libera su email de acceso mediante una dirección interna no entregable. No elimina personas CRM ni movimientos financieros y no cierra cajas automáticamente. El correo original queda asociado únicamente al nuevo usuario activo de plataforma en `SecurityUsers`.

## Cambios de esquema y compatibilidad

Migración `20261030041242_SuperAdministradorBootstrapSeguro`: rol/permisos nuevos y tabla de metadata global `SaasInicializaciones` (Clave varchar(50) PK, UsuarioId Guid, FechaCreacion UTC). No es una tabla de clientes y no tiene API pública. No se eliminan ni promueven usuarios existentes. Down se bloquea si existe una marca o asignación del rol reservado.

Se actualizaron las semillas del inicializador para incluir AdministradorEmpresa y SuperAdministrador; un rol SuperAdministrador ya deshabilitado no se reactiva en arranques sucesivos. Se preserva la identidad del admin legado y su configuración existente; ese usuario ya no puede confirmar pagos SaaS.

Durante la validación de instalación vacía apareció un error previo de SQL Server en `20260902041935_Sprint4_ReportesFinancieros_Y_ConsolidacionContable`: el backfill referenciaba columnas agregadas en el mismo batch. Se separaron DDL y UPDATE en dos comandos sin alterar su lógica. No vuelve a ejecutar esa migración en bases donde el historial ya la marca aplicada; habilita la instalación desde cero.

## Verificación

- Build de la solución: sin errores ni advertencias.
- 120 pruebas aprobadas: contraseña fuerte/hash, única inicialización, no reactivación/restablecimiento en reintentos, no promoción por email, rol no asignable y denegación al administrador legado para aprobar pagos, además de las suites anteriores.
- Prueba manual aislada sobre una base SQL NUEVA: todas las migraciones completas, CLI de bootstrap con credenciales sintéticas, segunda ejecución con otra contraseña sin cambios, exactamente una cuenta/una marca/un evento. La base QA se eliminó al finalizar. No se crearon credenciales de propietario en la base de trabajo.
- Sintaxis PowerShell validada sin ejecutar los prompts con credenciales del usuario.

Pendiente para el propietario: ejecutar el script con su correo y contraseña privados. No se configuró ninguna contraseña universal ni se guardaron credenciales nuevas en appsettings.
