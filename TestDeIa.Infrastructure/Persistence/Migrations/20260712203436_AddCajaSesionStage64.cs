using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCajaSesionStage64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CajaSesionId",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "CompraDetalles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaEmisionCompra",
                table: "CompraDetalles",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql("""
                UPDATE detalle
                SET
                    detalle.EmpresaId = compra.EmpresaId,
                    detalle.FechaEmisionCompra = compra.FechaEmision
                FROM CompraDetalles detalle
                INNER JOIN Compras compra ON compra.Id = detalle.CompraId
                """);

            migrationBuilder.Sql("""
                UPDATE factura
                SET factura.UsuarioId = empresa.OwnerUserId
                FROM Facturas factura
                INNER JOIN EmpresasEmisoras empresa ON empresa.Id = factura.EmpresaId
                WHERE factura.UsuarioId = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.CreateTable(
                name: "CajaSesiones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaApertura = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaCierre = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MontoApertura = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalVentasEfectivoCalculado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalVentasTarjetaCalculado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoFisicoEfectivoReal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoFisicoTarjetaReal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiferenciaEfectivo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiferenciaTarjeta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstadoCaja = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajaSesiones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemoriasAnalisisFiscal_EmpresaId_Anio_Mes_CreatedAt",
                table: "MemoriasAnalisisFiscal",
                columns: new[] { "EmpresaId", "Anio", "Mes", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_EmpresaId_FechaMovimiento_BodegaId_ProductoId",
                table: "KardexMovimientos",
                columns: new[] { "EmpresaId", "FechaMovimiento", "BodegaId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_CajaSesionId",
                table: "Facturas",
                column: "CajaSesionId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_CajaSesionId_CreatedAt",
                table: "Facturas",
                columns: new[] { "EmpresaId", "CajaSesionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CompraDetalles_EmpresaId_FechaEmisionCompra_ProductoId",
                table: "CompraDetalles",
                columns: new[] { "EmpresaId", "FechaEmisionCompra", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_CajaSesiones_EmpresaId_UsuarioId_EstadoCaja",
                table: "CajaSesiones",
                columns: new[] { "EmpresaId", "UsuarioId", "EstadoCaja" });

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_CajaSesiones_CajaSesionId",
                table: "Facturas",
                column: "CajaSesionId",
                principalTable: "CajaSesiones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_CajaSesiones_CajaSesionId",
                table: "Facturas");

            migrationBuilder.DropTable(
                name: "CajaSesiones");

            migrationBuilder.DropIndex(
                name: "IX_MemoriasAnalisisFiscal_EmpresaId_Anio_Mes_CreatedAt",
                table: "MemoriasAnalisisFiscal");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_EmpresaId_FechaMovimiento_BodegaId_ProductoId",
                table: "KardexMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_CajaSesionId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_CajaSesionId_CreatedAt",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_CompraDetalles_EmpresaId_FechaEmisionCompra_ProductoId",
                table: "CompraDetalles");

            migrationBuilder.DropColumn(
                name: "CajaSesionId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CompraDetalles");

            migrationBuilder.DropColumn(
                name: "FechaEmisionCompra",
                table: "CompraDetalles");
        }
    }
}
