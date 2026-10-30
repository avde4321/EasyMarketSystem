# Fase 2 — Registro público Blazor

## Acceso

- Rutas `/register-tenant` y `/registro`, componente `RegisterTenant.razor` con `EmptyLayout` y acceso anónimo.
- Enlace «¿Aún no tienes cuenta? Registra tu empresa» en `/login`.
- Se reutilizan colores, controles, botones e indicador `ProcessingOverlay` del sistema; distribución adaptable a una o dos columnas.

## Formulario y validación

Secciones Administrador, Datos fiscales y Plan. Usa directamente `PublicTenantRegisterDto` con `EditForm`, `DataAnnotationsValidator`, resumen y mensajes por campo. El RUC se trata como texto para preservar ceros iniciales y exige 13 dígitos ASCII. La contraseña exige 12–128 caracteres, mayúscula, minúscula, número y símbolo. Los mensajes principales del DTO se presentan en español.

Los planes se consultan desde `GET api/saas/plans`: no se codifican nombres, precios ni IDs en la pantalla. Se muestra un selector y resumen de límites. Se requiere selección explícita, con demo seleccionada como modalidad inicial. Si la carga falla, se puede reintentar y no es posible enviar un registro sin plan. Solo `OnValidSubmit` ejecuta el POST; el servicio backend vuelve a validar todo.

## API pública y errores

`PublicTenantApiClient` usa el cliente nombrado `PublicApi`, con la misma URL configurada de la API pero SIN los handlers de JWT/empresa/suscripción. Así no arrastra cookies de sesión de aplicación (tokens locales), JWT o `X-Empresa-Id` de una cuenta anterior. No hay reintentos automáticos del POST.

Reconoce ProblemDetails (`detail`, `title`) y errores de validación (`errors`), así como respuesta heredada `message`. HTTP 429 tiene mensaje propio; errores 5xx no exponen detalles internos. Respuestas HTML, vacías o JSON no objeto de error se convierten en una alerta; los errores de red/timeout se manejan en la vista. Los mensajes se renderizan como texto Razor, nunca HTML sin escapar.

Se bloquean campos y botón durante el registro y se muestra la animación estándar. Si se pierde la confirmación, se aconseja iniciar sesión antes de repetir la solicitud. Si el backend confirma el alta pero falla el almacenamiento del navegador, el formulario no permite repetir el registro y muestra cómo iniciar sesión.

## Sesión y navegación

Guarda la empresa activa mediante `EmpresaSessionService`, luego el JWT mediante `TokenAuthenticationStateProvider`, reutilizando el almacenamiento local existente. No se guarda la contraseña. Tras el éxito se limpia el campo contraseña y se navega con reemplazo de historial a `/dashboard`, alias agregado al panel `/` existente (no un dashboard duplicado).

Las rutas públicas se excluyen del guard de navegación por suscripción. El dashboard conserva ese guard: una demo activa accede normalmente y un plan `PendientePago` se redirige a `/subscription-expired`. No se simula un pago ni se relaja la seguridad de las operaciones.

## Pruebas

`PublicTenantApiClientTests`: prueba el código HTTP real mediante enlace de fuente, sin dependencias de ejecución WebAssembly ni paquetes adicionales. Cubre POST/DTO/respuesta de sesión, catálogo de planes, ProblemDetails 409, validación 400, rate limit 429, servidor 503 sin filtración de detalles, HTML inesperado y JSON no objeto. La suite backend anterior conserva las pruebas de atomicidad y concurrencia.

No requiere migraciones nuevas. La confirmación de email, cobro real y protección adicional contra abuso continúan fuera del alcance de esta fase.
