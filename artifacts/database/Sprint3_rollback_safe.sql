SET XACT_ABORT ON;
BEGIN TRANSACTION;

PRINT 'Sprint 3 rollback seguro iniciado.';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ColaProcesamientoSRI_EmpresaId_Estado' AND object_id = OBJECT_ID(N'[dbo].[ColaProcesamientoSRI]'))
    DROP INDEX [IX_ColaProcesamientoSRI_EmpresaId_Estado] ON [dbo].[ColaProcesamientoSRI];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ColaProcesamientoSRI_EmpresaId_ComprobanteId_TipoDocumentoId' AND object_id = OBJECT_ID(N'[dbo].[ColaProcesamientoSRI]'))
    DROP INDEX [IX_ColaProcesamientoSRI_EmpresaId_ComprobanteId_TipoDocumentoId] ON [dbo].[ColaProcesamientoSRI];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Compras_EmpresaId_ProveedorId_FechaEmision' AND object_id = OBJECT_ID(N'[dbo].[Compras]'))
    DROP INDEX [IX_Compras_EmpresaId_ProveedorId_FechaEmision] ON [dbo].[Compras];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Compras_EmpresaId_ProveedorId_EstadoCompra' AND object_id = OBJECT_ID(N'[dbo].[Compras]'))
    DROP INDEX [IX_Compras_EmpresaId_ProveedorId_EstadoCompra] ON [dbo].[Compras];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Compras_EmpresaId_EstadoCompra_FechaEmision' AND object_id = OBJECT_ID(N'[dbo].[Compras]'))
    DROP INDEX [IX_Compras_EmpresaId_EstadoCompra_FechaEmision] ON [dbo].[Compras];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Compras_EmpresaId_ClaveAccesoProveedor' AND object_id = OBJECT_ID(N'[dbo].[Compras]'))
    DROP INDEX [IX_Compras_EmpresaId_ClaveAccesoProveedor] ON [dbo].[Compras];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobanteCabecera_EmpresaId_FechaEmision' AND object_id = OBJECT_ID(N'[dbo].[ComprobanteCabecera]'))
    DROP INDEX [IX_ComprobanteCabecera_EmpresaId_FechaEmision] ON [dbo].[ComprobanteCabecera];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobanteCabecera_EmpresaId_Estado_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[ComprobanteCabecera]'))
    DROP INDEX [IX_ComprobanteCabecera_EmpresaId_Estado_CreatedAt] ON [dbo].[ComprobanteCabecera];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobanteCabecera_EmpresaId_Estado' AND object_id = OBJECT_ID(N'[dbo].[ComprobanteCabecera]'))
    DROP INDEX [IX_ComprobanteCabecera_EmpresaId_Estado] ON [dbo].[ComprobanteCabecera];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobanteCabecera_EmpresaId_ClaveAcceso' AND object_id = OBJECT_ID(N'[dbo].[ComprobanteCabecera]'))
    DROP INDEX [IX_ComprobanteCabecera_EmpresaId_ClaveAcceso] ON [dbo].[ComprobanteCabecera];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobanteCabecera_Estado_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[ComprobanteCabecera]'))
    PRINT 'Índice legado IX_ComprobanteCabecera_Estado_CreatedAt ya existe.';
ELSE IF OBJECT_ID(N'[dbo].[ComprobanteCabecera]', N'U') IS NOT NULL
    CREATE INDEX [IX_ComprobanteCabecera_Estado_CreatedAt] ON [dbo].[ComprobanteCabecera] ([Estado], [CreatedAt]);

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobanteCabecera_ClaveAcceso' AND object_id = OBJECT_ID(N'[dbo].[ComprobanteCabecera]'))
    PRINT 'Índice legado IX_ComprobanteCabecera_ClaveAcceso ya existe.';
ELSE IF OBJECT_ID(N'[dbo].[ComprobanteCabecera]', N'U') IS NOT NULL
    CREATE UNIQUE INDEX [IX_ComprobanteCabecera_ClaveAcceso] ON [dbo].[ComprobanteCabecera] ([ClaveAcceso]);

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobantesRetencion_EmpresaId_EstadoSRI' AND object_id = OBJECT_ID(N'[dbo].[ComprobantesRetencion]'))
    DROP INDEX [IX_ComprobantesRetencion_EmpresaId_EstadoSRI] ON [dbo].[ComprobantesRetencion];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobantesRetencion_EmpresaId_ClaveAcceso' AND object_id = OBJECT_ID(N'[dbo].[ComprobantesRetencion]'))
    DROP INDEX [IX_ComprobantesRetencion_EmpresaId_ClaveAcceso] ON [dbo].[ComprobantesRetencion];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprobantesRetencion_ClaveAcceso' AND object_id = OBJECT_ID(N'[dbo].[ComprobantesRetencion]'))
    PRINT 'Índice legado IX_ComprobantesRetencion_ClaveAcceso ya existe.';
ELSE IF OBJECT_ID(N'[dbo].[ComprobantesRetencion]', N'U') IS NOT NULL
    CREATE UNIQUE INDEX [IX_ComprobantesRetencion_ClaveAcceso] ON [dbo].[ComprobantesRetencion] ([ClaveAcceso]) WHERE [ClaveAcceso] IS NOT NULL;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CuentasPorPagar_EmpresaId_ProveedorId_EstadoDeuda' AND object_id = OBJECT_ID(N'[dbo].[CuentasPorPagar]'))
    DROP INDEX [IX_CuentasPorPagar_EmpresaId_ProveedorId_EstadoDeuda] ON [dbo].[CuentasPorPagar];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CuentasPorPagar_EmpresaId_EstadoDeuda_FechaVence' AND object_id = OBJECT_ID(N'[dbo].[CuentasPorPagar]'))
    DROP INDEX [IX_CuentasPorPagar_EmpresaId_EstadoDeuda_FechaVence] ON [dbo].[CuentasPorPagar];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_NumeroDocumentoRef_Monto' AND object_id = OBJECT_ID(N'[dbo].[ExtractoBancarioDetalles]'))
    DROP INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_NumeroDocumentoRef_Monto] ON [dbo].[ExtractoBancarioDetalles];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_FechaTransaccion' AND object_id = OBJECT_ID(N'[dbo].[ExtractoBancarioDetalles]'))
    DROP INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_FechaTransaccion] ON [dbo].[ExtractoBancarioDetalles];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_Conciliado_FechaTransaccion' AND object_id = OBJECT_ID(N'[dbo].[ExtractoBancarioDetalles]'))
    DROP INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_Conciliado_FechaTransaccion] ON [dbo].[ExtractoBancarioDetalles];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_Secuencial' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_Secuencial] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_FechaEmision' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_FechaEmision] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_Estado_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_Estado_CreatedAt] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_Estado' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_Estado] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_ClienteId' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_ClienteId] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_EmpresaId_ClaveAcceso' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    DROP INDEX [IX_Facturas_EmpresaId_ClaveAcceso] ON [dbo].[Facturas];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_Estado_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    PRINT 'Índice legado IX_Facturas_Estado_CreatedAt ya existe.';
ELSE IF OBJECT_ID(N'[dbo].[Facturas]', N'U') IS NOT NULL
    CREATE INDEX [IX_Facturas_Estado_CreatedAt] ON [dbo].[Facturas] ([Estado], [CreatedAt]);

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_Establecimiento_PuntoEmision_Secuencial' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    PRINT 'Índice legado IX_Facturas_Establecimiento_PuntoEmision_Secuencial ya existe.';
ELSE IF OBJECT_ID(N'[dbo].[Facturas]', N'U') IS NOT NULL
    CREATE UNIQUE INDEX [IX_Facturas_Establecimiento_PuntoEmision_Secuencial] ON [dbo].[Facturas] ([Establecimiento], [PuntoEmision], [Secuencial]);

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Facturas_ClaveAcceso' AND object_id = OBJECT_ID(N'[dbo].[Facturas]'))
    PRINT 'Índice legado IX_Facturas_ClaveAcceso ya existe.';
ELSE IF OBJECT_ID(N'[dbo].[Facturas]', N'U') IS NOT NULL
    CREATE UNIQUE INDEX [IX_Facturas_ClaveAcceso] ON [dbo].[Facturas] ([ClaveAcceso]);

-- No se eliminan EmpresaId/CuentaBancariaId de ExtractoBancarioDetalles en este rollback operativo:
-- son campos denormalizados, reconstruibles desde headers/cuentas y no afectan al modelo anterior si permanecen como columnas adicionales.
-- Si se requiere rollback EF estricto en ambiente no productivo, usar `dotnet ef database update 20260929040039_AddTesoreriaAndConciliacionBancaria`.

COMMIT TRANSACTION;
PRINT 'Sprint 3 rollback seguro finalizado.';
