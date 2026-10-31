BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_Facturas_ClaveAcceso] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_Facturas_Establecimiento_PuntoEmision_Secuencial] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_Facturas_Estado_CreatedAt] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_ComprobantesRetencion_ClaveAcceso] ON [ComprobantesRetencion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_ComprobanteCabecera_ClaveAcceso] ON [ComprobanteCabecera];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_ComprobanteCabecera_Estado_CreatedAt] ON [ComprobanteCabecera];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    ALTER TABLE [ExtractoBancarioDetalles] ADD [CuentaBancariaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    ALTER TABLE [ExtractoBancarioDetalles] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    UPDATE detalle
    SET
        detalle.[EmpresaId] = cuenta.[EmpresaId],
        detalle.[CuentaBancariaId] = header.[CuentaBancariaId]
    FROM [ExtractoBancarioDetalles] AS detalle
    INNER JOIN [ExtractoBancarioHeaders] AS header
        ON detalle.[ExtractoHeaderId] = header.[Id]
    INNER JOIN [CuentasBancarias] AS cuenta
        ON header.[CuentaBancariaId] = cuenta.[Id]
    WHERE detalle.[EmpresaId] = '00000000-0000-0000-0000-000000000000'
       OR detalle.[CuentaBancariaId] = '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_EmpresaId_ClaveAcceso] ON [Facturas] ([EmpresaId], [ClaveAcceso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_ClienteId] ON [Facturas] ([EmpresaId], [ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial] ON [Facturas] ([EmpresaId], [Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_Estado] ON [Facturas] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_Estado_CreatedAt] ON [Facturas] ([EmpresaId], [Estado], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_FechaEmision] ON [Facturas] ([EmpresaId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_Secuencial] ON [Facturas] ([EmpresaId], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_Conciliado_FechaTransaccion] ON [ExtractoBancarioDetalles] ([EmpresaId], [CuentaBancariaId], [Conciliado], [FechaTransaccion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_FechaTransaccion] ON [ExtractoBancarioDetalles] ([EmpresaId], [CuentaBancariaId], [FechaTransaccion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_NumeroDocumentoRef_Monto] ON [ExtractoBancarioDetalles] ([EmpresaId], [CuentaBancariaId], [NumeroDocumentoRef], [Monto]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_EmpresaId_EstadoDeuda_FechaVence] ON [CuentasPorPagar] ([EmpresaId], [EstadoDeuda], [FechaVence]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_EmpresaId_ProveedorId_EstadoDeuda] ON [CuentasPorPagar] ([EmpresaId], [ProveedorId], [EstadoDeuda]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComprobantesRetencion_EmpresaId_ClaveAcceso] ON [ComprobantesRetencion] ([EmpresaId], [ClaveAcceso]) WHERE [ClaveAcceso] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobantesRetencion_EmpresaId_EstadoSRI] ON [ComprobantesRetencion] ([EmpresaId], [EstadoSRI]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobanteCabecera_EmpresaId_ClaveAcceso] ON [ComprobanteCabecera] ([EmpresaId], [ClaveAcceso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_EmpresaId_Estado] ON [ComprobanteCabecera] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_EmpresaId_Estado_CreatedAt] ON [ComprobanteCabecera] ([EmpresaId], [Estado], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_EmpresaId_FechaEmision] ON [ComprobanteCabecera] ([EmpresaId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Compras_EmpresaId_ClaveAccesoProveedor] ON [Compras] ([EmpresaId], [ClaveAccesoProveedor]) WHERE [ClaveAccesoProveedor] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Compras_EmpresaId_EstadoCompra_FechaEmision] ON [Compras] ([EmpresaId], [EstadoCompra], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Compras_EmpresaId_ProveedorId_EstadoCompra] ON [Compras] ([EmpresaId], [ProveedorId], [EstadoCompra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Compras_EmpresaId_ProveedorId_FechaEmision] ON [Compras] ([EmpresaId], [ProveedorId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ColaProcesamientoSRI_EmpresaId_ComprobanteId_TipoDocumentoId] ON [ColaProcesamientoSRI] ([EmpresaId], [ComprobanteId], [TipoDocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ColaProcesamientoSRI_EmpresaId_Estado] ON [ColaProcesamientoSRI] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes', N'9.0.9');
END;

COMMIT;
GO

