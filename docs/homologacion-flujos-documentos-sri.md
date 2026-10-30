# Evolución de firma y XML por documento electrónico

## Alcance implementado

Se conservan estructuras distintas: factura 01, liquidación de compra 03, nota de crédito 04 y retención ATS 07. No se convierte un documento a la estructura de factura para poder firmarlo.

| Documento | Generación/validación | Procesamiento |
|---|---|---|
| 01 | Generador y XSD existentes | Se conserva su procesador y coordinador actuales. La cola documental, si contiene una factura, solo archiva y refleja una autorización ya existente; no autoriza por cuenta propia. |
| 03 | Generador propio; validación XSD completa para 1.0.0 y 1.1.0; nuevas emisiones conservan la versión 1.0.0 existente | Firma compartida, recepción, autorización y persistencia del XML autorizado en DocumentoAdjunto. Se corrige el orden de contribuyenteEspecial en la cabecera y se conserva el XML base al autorizar. |
| 04 | Generador y XSD 1.1.0 propios, con documento modificado y motivo | La cola ya ejecuta firma, recepción y autorización reales; deja de autorizar incondicionalmente. |
| 07 | Generador ATS 2.0.0 separado y XSD específico oficial | Toma totales, IVA, sustento y forma de pago de la compra vinculada; firma, recepción, autorización y archivos en DocumentoAdjunto. |

## Desarrollo frente a SRI real

- `ModoDesarrollo = true`: se mantiene la simulación interna explícita, sin llamadas al SRI. No se presenta una firma simulada nueva como si fuera una firma criptográfica real en las notas de crédito.
- `ModoDesarrollo = false`: no se autoriza sin respuesta AUTORIZADO, número y fecha del SRI. Ambiente 1/2 selecciona pruebas/producción y no equivale a simulación.
- La empresa real no se inició ni se enviaron comprobantes durante la implementación. Las pruebas HTTP usan respuestas controladas, no conexiones SRI.

## Reintentos, seguridad e integridad

- Se reutilizan el firmador XAdES-BES y el cliente SOAP corregidos.
- El servicio de envío común consume XML ya firmado: no construye contenido fiscal.
- Se comprueba ambiente XML/clave, presencia de firma y coincidencia de clave persistida con la firmada.
- Al reintentar con firma persistida se consulta autorización primero, conservando número, clave y firma. Los casos de recepción duplicada/en procesamiento se consultan nuevamente, sin generar otro secuencial.
- Una respuesta sin autorización se mantiene pendiente; no se trata como autorización ni rechazo definitivo.
- Se conserva el XML firmado antes de transmitir y el autorizado como adjunto separado.
- Se agregan bloqueos distribuidos por comprobante mediante `sp_getapplock`, con dueño Session, sin mantener una transacción SQL durante HTTP. La cola recupera reclamaciones vencidas y respeta NextRetryAt.
- El coordinador de compras conserva el contexto de sistema necesario en lotes. Las liquidaciones firmadas pendientes ya no quedan excluidas de recuperación.

## Retenciones: datos que no deben inventarse

La versión ATS necesita información que las bases de retención solas no contienen. Se elimina la construcción incompleta que sumaba bases de Renta e IVA como total del documento y generaba un contenedor de impuestos vacío.

El flujo implementado corresponde a una compra local vinculada, con IVA representado en sus detalles, sin reembolsos ni tratamientos internacionales especiales. Se bloquea emisión real si no existe la compra, faltan forma de pago/sustento o los impuestos no coinciden. Una retención manual sin compra puede mantenerse como borrador, pero no emitirse inventando sus totales. Extender a múltiples sustentos, pagos al exterior, dividendos o reembolsos requiere incorporar primero sus datos fiscales específicos.

Se incorpora `ParteRelacionada` (bit, default false) y se permite configurarla en **Retenciones → Ver → Datos fiscales**, únicamente antes de firmar. Los registros existentes conservan el valor predeterminado: el contador debe revisar esta clasificación antes de emitir. No se han cambiado los catálogos ni porcentajes existentes de retención; su revisión fiscal sigue siendo un control previo al despliegue productivo.

La descarga XML de retenciones entrega el artefacto real disponible (autorizado, firmado o generado), no una reconstrucción parcial de diagnóstico. Si no existe, informa que debe procesarse primero.

## Base de datos

Migración incremental `20261029034941_RetencionParteRelacionada`, aplicada en desarrollo (`TestDeIaCrm`). Agrega únicamente el campo bit con valor por defecto; no elimina documentos, columnas ni firmas anteriores. No se renumeraron comprobantes ni se reprocesaron datos históricos.

Consulta local de auditoría: no se encontraron notas de crédito marcadas AUTORIZADO sin número de autorización. En otros entornos conviene revisar ese caso antes de desplegar: la corrección no invalida ni reenvía automáticamente documentos históricos.

## Validación y límites de certificación

- `dotnet build TestDeIa.sln --no-restore`: 0 errores, 0 warnings.
- `dotnet test TestDeIa.Tests/TestDeIa.Tests.csproj --no-restore`: 54 pruebas satisfactorias.
- Cobertura añadida: firma para cuatro raíces, SOAP con parámetros sin namespace, reintentos por consulta, ausencia de autorización, rechazo, XSD de liquidación y retención, cultura decimal española, totales de compra sin duplicar bases, estructura propia de nota de crédito, simulación explícita y procesamiento completo de nota de crédito con respuestas controladas.
- EF InMemory y HTTP controlado no sustituyen pruebas concurrentes SQL Server ni aceptación real del SRI. La validación en ambiente SRI de pruebas con certificado de cada empresa continúa pendiente.
- No se agregaron librerías NuGet.

## Fuentes de los esquemas

Descargados desde los enlaces de **Información técnica y guías** en el [portal oficial SRI](https://www.sri.gob.ec/facturacion-electronica):

- [Liquidación de compra 1.0.0 / 1.1.0](https://www.sri.gob.ec/o/sri-portlet-biblioteca-alfresco-internet/descargar/ee386507-04f8-4a45-b9cd-6d4e4c6ac1e6/liquidacion.zip).
- [Retención 2.0.0](https://www.sri.gob.ec/o/sri-portlet-biblioteca-alfresco-internet/descargar/90950fca-73a7-4cfb-9c2d-3142b10435f2/retencion.zip).
- Dependencia XMLDSIG del [W3C](https://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd), embebida localmente y sin resolución de recursos externos en ejecución.
