using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_ProductoBodega_Y_KardexAppendOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "StockActual",
                table: "ProductosBodega",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<bool>(
                name: "EsActivo",
                table: "ProductosBodega",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockMaximo",
                table: "ProductosBodega",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StockMinimo",
                table: "ProductosBodega",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "TipoMovimiento",
                table: "KardexMovimientos",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "FechaMovimiento",
                table: "KardexMovimientos",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CompraId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoTotal",
                table: "KardexMovimientos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FacturaId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockAnterior",
                table: "KardexMovimientos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StockNuevo",
                table: "KardexMovimientos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferenciaInventarioId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE KardexMovimientos
                SET TipoMovimiento = CASE
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('ENTRADA', 'INGRESO_COMPRA') THEN 'ENTRADA_COMPRA'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('SALIDA', 'EGRESO_FACTURA') THEN 'SALIDA_VENTA'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('DEVOLUCION', 'DEVOLUCIONVENTA', 'DEVOLUCION_VENTA') THEN 'DEVOLUCION_VENTA'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('INGRESO_AJUSTE') THEN 'AJUSTE_INGRESO'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('EGRESO_AJUSTE') THEN 'AJUSTE_EGRESO'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('EGRESO_MERMA', 'MERMA') THEN 'MERMA_INVENTARIO'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('TRANSFERENCIA_DESPACHO', 'TRANSFERENCIA_SALIDA') THEN 'TRANSFERENCIA_SALIDA'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('TRANSFERENCIA_RECEPCION', 'TRANSFERENCIA_ENTRADA') THEN 'TRANSFERENCIA_ENTRADA'
                    WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('TOMA_FISICA') THEN 'TOMA_FISICA'
                    ELSE 'AJUSTE_INGRESO'
                END;

                UPDATE KardexMovimientos
                SET
                    StockNuevo = SaldoCantidad,
                    StockAnterior = CASE
                        WHEN CantidadEntrada > 0 THEN SaldoCantidad - CantidadEntrada
                        WHEN CantidadSalida > 0 THEN SaldoCantidad + CantidadSalida
                        ELSE SaldoCantidad
                    END,
                    CostoTotal = CASE
                        WHEN CantidadEntrada > 0 THEN CantidadEntrada * CostoUnitario
                        WHEN CantidadSalida > 0 THEN CantidadSalida * CostoUnitario
                        ELSE 0
                    END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosBodega_EmpresaId_BodegaId_ProductoId",
                table: "ProductosBodega",
                columns: new[] { "EmpresaId", "BodegaId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_CompraId",
                table: "KardexMovimientos",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_EmpresaId_BodegaId_ProductoId_FechaMovimiento",
                table: "KardexMovimientos",
                columns: new[] { "EmpresaId", "BodegaId", "ProductoId", "FechaMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_FacturaId",
                table: "KardexMovimientos",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_TransferenciaInventarioId",
                table: "KardexMovimientos",
                column: "TransferenciaInventarioId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KardexMovimientos_TipoMovimiento",
                table: "KardexMovimientos",
                sql: "[TipoMovimiento] IN ('ENTRADA_COMPRA', 'SALIDA_VENTA', 'DEVOLUCION_VENTA', 'AJUSTE_INGRESO', 'AJUSTE_EGRESO', 'TRANSFERENCIA_ENTRADA', 'TRANSFERENCIA_SALIDA', 'MERMA_INVENTARIO', 'TOMA_FISICA')");

            migrationBuilder.AddForeignKey(
                name: "FK_KardexMovimientos_Compras_CompraId",
                table: "KardexMovimientos",
                column: "CompraId",
                principalTable: "Compras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KardexMovimientos_Facturas_FacturaId",
                table: "KardexMovimientos",
                column: "FacturaId",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KardexMovimientos_TransferenciasInventario_TransferenciaInventarioId",
                table: "KardexMovimientos",
                column: "TransferenciaInventarioId",
                principalTable: "TransferenciasInventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KardexMovimientos_Compras_CompraId",
                table: "KardexMovimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_KardexMovimientos_Facturas_FacturaId",
                table: "KardexMovimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_KardexMovimientos_TransferenciasInventario_TransferenciaInventarioId",
                table: "KardexMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_ProductosBodega_EmpresaId_BodegaId_ProductoId",
                table: "ProductosBodega");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_CompraId",
                table: "KardexMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_EmpresaId_BodegaId_ProductoId_FechaMovimiento",
                table: "KardexMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_FacturaId",
                table: "KardexMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_TransferenciaInventarioId",
                table: "KardexMovimientos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KardexMovimientos_TipoMovimiento",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "EsActivo",
                table: "ProductosBodega");

            migrationBuilder.DropColumn(
                name: "StockMaximo",
                table: "ProductosBodega");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "ProductosBodega");

            migrationBuilder.DropColumn(
                name: "CompraId",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "CostoTotal",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "FacturaId",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "StockAnterior",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "StockNuevo",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "TransferenciaInventarioId",
                table: "KardexMovimientos");

            migrationBuilder.AlterColumn<decimal>(
                name: "StockActual",
                table: "ProductosBodega",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "TipoMovimiento",
                table: "KardexMovimientos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "FechaMovimiento",
                table: "KardexMovimientos",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
