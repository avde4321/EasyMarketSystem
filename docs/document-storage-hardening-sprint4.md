# Sprint 4 Robustecimiento SaaS - Gestión documental, storage cifrado y aislamiento por empresa

## Alcance implementado

Se evolucionó el almacenamiento documental existente hacia un servicio tenant-aware con validación de seguridad y cifrado para archivos sensibles.

## Servicio de storage

Se agregó `TenantStorageService` y se mantuvo `LocalDocumentoStorageService` como wrapper compatible.

Contratos actualizados:

- `IDocumentoStorageService`
- `ITenantStorageService`
- `DocumentoStorageRequest`
- `DocumentoStorageResult`
- `DocumentoStorageReadResult`

## Estructura física aislada

Los documentos se guardan bajo la estructura:

```text
/storage/{EmpresaId}/{Modulo}/{EntidadTipo}/{yyyy}/{MM}/{EntidadId}/{NombreArchivo}
```

Se utiliza `EmpresaId` en formato Guid con guiones para facilitar trazabilidad humana y evitar mezcla entre tenants.

## Metadata documental

La entidad `DocumentoAdjunto` ahora soporta:

- `NombreArchivo`
- `ContentType`
- `RutaStorage`
- `HashSHA256`
- `TamanoBytes`
- `Version`
- `Origen`
- `CreadoPorUsuarioId`
- `FechaCreacion`
- `EsCifrado`
- `AlgoritmoCifrado`

Migración generada:

- `20260930050656_Sprint4_TenantStorage_CifradoDocumental`

## Cifrado

Se implementó cifrado simétrico:

- Algoritmo: `AES-256-GCM`.
- Hash SHA256 calculado sobre el contenido original antes de cifrar.
- Verificación de integridad al leer/descifrar mediante `ReadAsync`.
- El archivo persistido no queda en claro cuando `RequiereCifrado = true`.

## Configuración segura

La clave de cifrado no debe guardarse en `appsettings.json`.

En desarrollo:

```powershell
dotnet user-secrets set "DocumentoStorage:EncryptionKeyBase64" "<base64-de-32-bytes>" --project TestDeIa.Api
```

En QA/Producción:

- usar variable de entorno `DocumentoStorage__EncryptionKeyBase64`; o
- Azure Key Vault / proveedor equivalente.

La clave debe ser exactamente de 32 bytes antes de codificarla en Base64.

## Validaciones de subida

Lista blanca de extensiones:

- `.xml`
- `.pdf`
- `.p12`
- `.pfx`
- `.txt`
- `.csv`
- `.xlsx`

Validación MIME estricta según extensión.

Límites configurables:

- `DocumentoStorage:DefaultMaxBytes`
- `DocumentoStorage:CertificateMaxBytes`

Valores actuales:

- documentos generales: 10 MB;
- certificados: 5 MB.

## Lo que se mantuvo por compatibilidad

No se eliminaron todavía columnas legacy como:

- `Facturas.XmlGenerado`
- `Facturas.XmlFirmado`
- `Compras.XmlGenerado`
- `Compras.XmlFirmado`
- `ComprobanteCabecera.XmlGenerado`
- `ComprobanteCabecera.XmlFirmado`
- `EmpresaEmisora.CertificadoContenido`
- `CertificadoDigitalEmpresa.Contenido`

Motivo: esos campos siguen siendo usados por flujos de SRI, RIDE, email y compatibilidad histórica. Eliminarlos ahora sería un cambio destructivo. La estrategia correcta es migración gradual: guardar nuevas versiones en `DocumentoAdjunto`, adaptar lectores, verificar producción y luego planificar limpieza controlada.

## Scripts generados

- `artifacts/database/Sprint4_TenantStorage_CifradoDocumental_delta_idempotent.sql`
- `artifacts/database/TestDeIa_idempotent.sql`

## Validación

- `dotnet build TestDeIa.sln --no-restore -v:minimal`: 0 errores, 0 warnings.
- `dotnet test TestDeIa.sln --no-build -v:minimal`: 10 pruebas pasadas.

## Recomendaciones DBA / Seguridad

- Mantener la clave AES fuera del repositorio y rotarla con procedimiento formal.
- Hacer backup de storage y base de datos de forma coordinada, porque `DocumentoAdjunto` contiene metadata y el filesystem/blob contiene el binario.
- Activar monitoreo sobre crecimiento de `/storage`.
- Para producción cloud, reemplazar el backend local por Blob Storage/S3 compatible manteniendo el contrato `ITenantStorageService`.
- Planificar un sprint posterior para extraer gradualmente XML, PDF y certificados legacy desde SQL hacia `DocumentoAdjunto`.
