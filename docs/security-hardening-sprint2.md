# Sprint 2 Robustecimiento SaaS - Hardening de Seguridad

## Alcance implementado

- Se retiró el secreto JWT plano de `appsettings.Development.json`.
- Se agregó resolución centralizada de claves JWT con soporte de rotación mediante `Security:Jwt:SigningKeys`.
- La emisión de tokens usa la primera clave activa como primaria e incluye `kid`.
- La validación Bearer acepta todas las claves activas configuradas y mantiene validación estricta de issuer, audience, lifetime, firma y algoritmo.
- Se agregaron políticas dedicadas de rate limiting para:
  - Login: `login`.
  - Integraciones masivas/SRI/emisión: `integracion-masiva`.
  - Webhook de pagos: `webhook-pago`.
- Se marcaron endpoints sensibles con rate limiting dedicado:
  - `api/security/login`.
  - Emisión de facturas y notas de crédito.
  - Carga masiva XML SRI.
  - Procesamiento/reintento de retenciones SRI.
  - Generación de links de pago y webhook PayPhone.
  - Análisis de factura proveedor.
- Se reforzaron cabeceras HTTP de seguridad agregando `Content-Security-Policy`.
- Se habilitó HSTS para ambientes no desarrollo.
- Se agregaron pruebas automatizadas de seguridad en `TestDeIa.Tests`.

## Configuración requerida fuera del repositorio

En desarrollo, configurar la clave JWT mediante User Secrets:

```powershell
dotnet user-secrets set "Security:Jwt:SigningKeys:0:KeyId" "dev-local-2026" --project TestDeIa.Api
dotnet user-secrets set "Security:Jwt:SigningKeys:0:Secret" "<clave-de-32-bytes-o-mas>" --project TestDeIa.Api
dotnet user-secrets set "Security:Jwt:SigningKeys:0:IsActive" "true" --project TestDeIa.Api
```

En QA/Producción, usar variables de entorno o Key Vault:

- `Security__Jwt__SigningKeys__0__KeyId`
- `Security__Jwt__SigningKeys__0__Secret`
- `Security__Jwt__SigningKeys__0__IsActive`

Para rotar claves, agregar una nueva clave como índice `0` y mantener temporalmente la anterior como índice `1` activa hasta que expiren los tokens emitidos previamente.

## Lo que se mantuvo igual

- Se conservó el modelo actual de auditoría de login exitoso/fallido y bloqueo temporal por intentos fallidos.
- Se mantuvo la política CORS existente basada en dominios autorizados.
- Se mantuvo la política global de autorización ya configurada para requerir usuario autenticado por defecto.

## Validación

- `dotnet build TestDeIa.sln --no-restore -v:minimal`: 0 errores, 0 warnings.
- `dotnet test TestDeIa.sln --no-build -v:minimal`: 7 pruebas pasadas.
