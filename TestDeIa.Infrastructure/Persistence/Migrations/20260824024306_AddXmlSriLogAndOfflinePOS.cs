using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddXmlSriLogAndOfflinePOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[FacturaCompraXmlLogs] (
                        [Id] uniqueidentifier NOT NULL,
                        [EmpresaId] uniqueidentifier NOT NULL,
                        [ClaveAcceso] nvarchar(49) NOT NULL,
                        [RucEmisor] nvarchar(13) NOT NULL,
                        [RazonSocialEmisor] nvarchar(300) NOT NULL,
                        [RucComprador] nvarchar(13) NOT NULL,
                        [FechaEmision] datetimeoffset NOT NULL,
                        [CodDoc] nvarchar(2) NOT NULL,
                        [EstabPuntoEmiSecuencial] nvarchar(17) NOT NULL,
                        [TotalSinImpuestos] decimal(18,4) NOT NULL,
                        [TotalDescuento] decimal(18,4) NOT NULL,
                        [ImporteTotal] decimal(18,4) NOT NULL,
                        [XmlContenido] nvarchar(max) NOT NULL,
                        [EstadoProcesamiento] tinyint NOT NULL,
                        [CompraId] uniqueidentifier NULL,
                        [CreatedAt] datetimeoffset NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_CreatedAt] DEFAULT SYSUTCDATETIME(),
                        [UpdatedAt] datetimeoffset NULL,
                        CONSTRAINT [PK_FacturaCompraXmlLogs] PRIMARY KEY ([Id])
                    );
                END;

                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'EmpresaId') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [EmpresaId] uniqueidentifier NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_EmpresaId] DEFAULT '00000000-0000-0000-0000-000000000000';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'ClaveAcceso') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [ClaveAcceso] nvarchar(49) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_ClaveAcceso] DEFAULT '';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'RucEmisor') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [RucEmisor] nvarchar(13) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_RucEmisor] DEFAULT '';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'RazonSocialEmisor') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [RazonSocialEmisor] nvarchar(300) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_RazonSocialEmisor] DEFAULT '';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'RucComprador') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [RucComprador] nvarchar(13) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_RucComprador] DEFAULT '';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'FechaEmision') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [FechaEmision] datetimeoffset NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_FechaEmision] DEFAULT SYSUTCDATETIME();
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'CodDoc') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [CodDoc] nvarchar(2) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_CodDoc] DEFAULT '01';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'EstabPuntoEmiSecuencial') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [EstabPuntoEmiSecuencial] nvarchar(17) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_EstabPuntoEmiSecuencial] DEFAULT '';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'TotalSinImpuestos') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [TotalSinImpuestos] decimal(18,4) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_TotalSinImpuestos] DEFAULT 0;
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'TotalDescuento') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [TotalDescuento] decimal(18,4) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_TotalDescuento] DEFAULT 0;
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'ImporteTotal') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [ImporteTotal] decimal(18,4) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_ImporteTotal] DEFAULT 0;
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'XmlContenido') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [XmlContenido] nvarchar(max) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_XmlContenido] DEFAULT '';
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'EstadoProcesamiento') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [EstadoProcesamiento] tinyint NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_EstadoProcesamiento] DEFAULT 0;
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'CompraId') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [CompraId] uniqueidentifier NULL;
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'CreatedAt') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [CreatedAt] datetimeoffset NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_CreatedAt_Add] DEFAULT SYSUTCDATETIME();
                IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'UpdatedAt') IS NULL
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [UpdatedAt] datetimeoffset NULL;

                IF OBJECT_ID(N'[dbo].[Compras]', N'U') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_FacturaCompraXmlLogs_Compras_CompraId')
                BEGIN
                    ALTER TABLE [dbo].[FacturaCompraXmlLogs]
                    ADD CONSTRAINT [FK_FacturaCompraXmlLogs_Compras_CompraId]
                    FOREIGN KEY ([CompraId]) REFERENCES [dbo].[Compras]([Id]) ON DELETE SET NULL;
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_CompraId' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
                    CREATE INDEX [IX_FacturaCompraXmlLogs_CompraId] ON [dbo].[FacturaCompraXmlLogs]([CompraId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
                    CREATE UNIQUE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso] ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [ClaveAcceso]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
                    CREATE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision] ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [EstadoProcesamiento], [FechaEmision]);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [dbo].[Catalogos] WHERE [Id] = '70000000-0000-0000-0000-000000000008')
                    INSERT INTO [dbo].[Catalogos] ([Id], [Codigo], [Descripcion], [IsActive], [Nombre]) VALUES ('70000000-0000-0000-0000-000000000008', N'RETENCION_IVA_SRI', N'Codigos base de retencion de IVA para documentos de proveedor.', 1, N'Retenciones IVA SRI');
                IF NOT EXISTS (SELECT 1 FROM [dbo].[Catalogos] WHERE [Id] = '70000000-0000-0000-0000-000000000009')
                    INSERT INTO [dbo].[Catalogos] ([Id], [Codigo], [Descripcion], [IsActive], [Nombre]) VALUES ('70000000-0000-0000-0000-000000000009', N'RETENCION_RENTA_SRI', N'Codigos base de retencion en la fuente para documentos de proveedor.', 1, N'Retenciones Renta SRI');

                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'0') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000081', '70000000-0000-0000-0000-000000000008', N'0', N'0%', 1, N'Sin retencion IVA', 1, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'10') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000082', '70000000-0000-0000-0000-000000000008', N'10', N'10%', 1, N'Retencion IVA 10%', 2, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'20') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000083', '70000000-0000-0000-0000-000000000008', N'20', N'20%', 1, N'Retencion IVA 20%', 3, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'30') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000084', '70000000-0000-0000-0000-000000000008', N'30', N'30%', 1, N'Retencion IVA 30%', 4, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'50') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000085', '70000000-0000-0000-0000-000000000008', N'50', N'50%', 1, N'Retencion IVA 50%', 5, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'70') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000086', '70000000-0000-0000-0000-000000000008', N'70', N'70%', 1, N'Retencion IVA 70%', 6, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'100') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000087', '70000000-0000-0000-0000-000000000008', N'100', N'100%', 1, N'Retencion IVA 100%', 7, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'0') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000091', '70000000-0000-0000-0000-000000000009', N'0', N'0%', 1, N'Sin retencion renta', 1, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'312') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000092', '70000000-0000-0000-0000-000000000009', N'312', N'1.75%', 1, N'Retencion renta codigo 312', 2, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'320') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000093', '70000000-0000-0000-0000-000000000009', N'320', N'1.75%', 1, N'Retencion renta codigo 320', 3, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'322') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000094', '70000000-0000-0000-0000-000000000009', N'322', N'1.75%', 1, N'Retencion renta codigo 322', 4, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'332') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000095', '70000000-0000-0000-0000-000000000009', N'332', N'1.75%', 1, N'Bienes codigo 332', 5, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'343') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000096', '70000000-0000-0000-0000-000000000009', N'343', N'2.75%', 1, N'Servicios codigo 343', 6, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'344') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000097', '70000000-0000-0000-0000-000000000009', N'344', N'2.75%', 1, N'Servicios codigo 344', 7, NULL);
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'3440') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000098', '70000000-0000-0000-0000-000000000009', N'3440', N'70%', 1, N'Retencion IVA codigo 3440', 8, NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[FacturaCompraXmlLogs];
                """);
        }
    }
}
