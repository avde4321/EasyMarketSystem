# Numeración de comprobantes y migración desde otro proveedor

## Acceso y uso

Seleccionar la empresa activa y abrir **Configuración → Numeración** (`/empresa/numeracion`). También existe acceso desde Mis empresas. Se requiere el permiso existente `EmpresaConfigurar`.

1. Elegir Pruebas (1) o Producción (2).
2. Identificar establecimiento, punto y documento.
3. Registrar el **último** secuencial utilizado por el proveedor anterior y el motivo.
4. Revisar el próximo número mostrado y confirmar.

Ejemplo: último `000001250` → próximo `000001251`. Cero significa comenzar en `000000001`. El ambiente elegido en esta pantalla configura el contador; **no cambia** el ambiente de emisión de la empresa.

No existe consulta automática del último número del SRI. El responsable debe confirmar el dato y coordinar el corte con el proveedor anterior. No deben emitir dos proveedores con la misma serie y ambiente simultáneamente.

## Cambios implementados

- Reutilización de `FacturaSecuenciales`, sin contador paralelo; incorpora `AmbienteSri`.
- Clave única: empresa + ambiente + documento + establecimiento + punto.
- Flujos 01 (factura), 03 (liquidación), 04 (nota de crédito) y 07 (retención) usan el mismo reservador.
- Reserva dentro de la transacción del comprobante y `sp_getapplock` exclusivo por serie/documento/ambiente. El bloqueo dura hasta commit/rollback y funciona entre instancias SQL Server.
- Protección contra retrocesos, números de más de nueve dígitos, puntos de otra empresa y formularios desactualizados. También se considera el máximo histórico persistido, aunque falte el contador.
- Historial de cambios con empresa, usuario, fecha, valores anterior/nuevo y motivo; consulta de los últimos 50 cambios.
- Retenciones ya no calculan su número únicamente con MAX; la creación manual debe respetar el siguiente número reservado.
- Índices únicos de documentos incluyen ambiente. Compras agrega un snapshot de ambiente utilizado para liquidaciones.

## Qué se conservó

- Documentos emitidos, sus números, XML y claves de acceso no se modifican.
- Puntos de emisión y permisos existentes; no se crean puntos ni bodegas automáticamente.
- Reprocesamiento SRI mantiene sus números y no reserva un nuevo secuencial.
- Los flujos existentes sin configuración previa conservan el inicio automático en 1 o el máximo ya utilizado. Esta entrega no obliga a todos los clientes actuales a realizar un onboarding adicional.

## Migración / DBA

`20261029031910_ConfigurarNumeracionComprobantesPorAmbiente` agrega dos campos, la tabla de auditoría y sustituye índices únicos sin eliminar columnas ni documentos. Aplicada en la base local de desarrollo `TestDeIaCrm`.

El contador legado, que no distinguía ambientes, se conserva en el ambiente actual de la empresa; el máximo histórico de cada ambiente se verifica adicionalmente al emitir. Si una empresa alternó ambientes antes de esta migración, revisar ambos contadores antes del corte productivo. Las liquidaciones con clave existente recuperan su ambiente desde la clave. No se reconstruye ni renumera ningún XML.

El rollback bloquea la pérdida de auditoría o la fusión de numeraciones incompatibles entre ambientes. En producción, revisar respaldo, ventana de despliegue y tamaño de los índices antes de aplicar; detener instancias antiguas que aún usan el algoritmo anterior. No usar rollback como operación rutinaria tras comenzar la emisión independiente por ambiente.

## Validación

- Compilación de solución: 0 errores y 0 advertencias.
- Suite automatizada: 39 pruebas satisfactorias; cubre continuidad, ambientes independientes, historial, máximo histórico, retrocesos, edición desactualizada y aislamiento multiempresa.
- Las nuevas pruebas de reglas usan EF InMemory: no sustituyen una prueba de carga multi-instancia SQL Server.
- No se enviaron comprobantes al SRI durante la implementación.

Para la prueba visual: reiniciar API/cliente, entrar a la ruta, seleccionar Pruebas y configurar una serie de ensayo. Verificar la vista previa y luego emitir un documento controlado. No cambiar contadores de producción para ensayar.
