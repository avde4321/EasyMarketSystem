BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930050656_Sprint4_TenantStorage_CifradoDocumental'
)
BEGIN
    ALTER TABLE [DocumentosAdjuntos] ADD [AlgoritmoCifrado] varchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930050656_Sprint4_TenantStorage_CifradoDocumental'
)
BEGIN
    ALTER TABLE [DocumentosAdjuntos] ADD [EsCifrado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930050656_Sprint4_TenantStorage_CifradoDocumental'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930050656_Sprint4_TenantStorage_CifradoDocumental', N'9.0.9');
END;

COMMIT;
GO

