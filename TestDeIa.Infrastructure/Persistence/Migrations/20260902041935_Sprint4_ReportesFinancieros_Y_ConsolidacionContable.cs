using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_ReportesFinancieros_Y_ConsolidacionContable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[FacturaDetalles]', N'CostoHistoricoTotal') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[FacturaDetalles]
                    ADD [CostoHistoricoTotal] decimal(18,4) NOT NULL
                    CONSTRAINT [DF_FacturaDetalles_CostoHistoricoTotal] DEFAULT 0;
                END;

                IF COL_LENGTH(N'[dbo].[FacturaDetalles]', N'CostoHistoricoUnitario') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[FacturaDetalles]
                    ADD [CostoHistoricoUnitario] decimal(18,6) NOT NULL
                    CONSTRAINT [DF_FacturaDetalles_CostoHistoricoUnitario] DEFAULT 0;
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_FacturaDetalles_ProductoId_FacturaId'
                      AND object_id = OBJECT_ID(N'[dbo].[FacturaDetalles]')
                )
                BEGIN
                    CREATE INDEX [IX_FacturaDetalles_ProductoId_FacturaId]
                    ON [dbo].[FacturaDetalles] ([ProductoId], [FacturaId]);
                END;

                UPDATE detalle
                SET
                    CostoHistoricoTotal = ISNULL(kardex.CostoTotal, 0),
                    CostoHistoricoUnitario = CASE
                        WHEN detalle.Cantidad > 0 THEN ISNULL(kardex.CostoTotal, 0) / detalle.Cantidad
                        ELSE 0
                    END
                FROM FacturaDetalles detalle
                OUTER APPLY (
                    SELECT SUM(movimiento.CostoTotal) AS CostoTotal
                    FROM KardexMovimientos movimiento
                    WHERE movimiento.FacturaId = detalle.FacturaId
                      AND movimiento.ProductoId = detalle.ProductoId
                      AND movimiento.TipoMovimiento IN ('SALIDA_VENTA', 'Salida')
                ) kardex
                WHERE detalle.CostoHistoricoTotal = 0
                  AND kardex.CostoTotal IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_FacturaDetalles_ProductoId_FacturaId'
                      AND object_id = OBJECT_ID(N'[dbo].[FacturaDetalles]')
                )
                BEGIN
                    DROP INDEX [IX_FacturaDetalles_ProductoId_FacturaId] ON [dbo].[FacturaDetalles];
                END;

                IF COL_LENGTH(N'[dbo].[FacturaDetalles]', N'CostoHistoricoTotal') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[FacturaDetalles] DROP CONSTRAINT IF EXISTS [DF_FacturaDetalles_CostoHistoricoTotal];
                    ALTER TABLE [dbo].[FacturaDetalles] DROP COLUMN [CostoHistoricoTotal];
                END;

                IF COL_LENGTH(N'[dbo].[FacturaDetalles]', N'CostoHistoricoUnitario') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[FacturaDetalles] DROP CONSTRAINT IF EXISTS [DF_FacturaDetalles_CostoHistoricoUnitario];
                    ALTER TABLE [dbo].[FacturaDetalles] DROP COLUMN [CostoHistoricoUnitario];
                END;
                """);
        }
    }
}
