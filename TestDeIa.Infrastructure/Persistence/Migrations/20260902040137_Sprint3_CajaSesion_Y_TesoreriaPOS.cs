using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_CajaSesion_Y_TesoreriaPOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVentasTransferenciaCalculado",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVentasTarjetaCalculado",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVentasEfectivoCalculado",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFisicoTransferenciaReal",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFisicoTarjetaReal",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFisicoEfectivoReal",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoApertura",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiferenciaTransferencia",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiferenciaTarjeta",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiferenciaEfectivo",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Diferencia",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "BodegaId",
                table: "CajaSesiones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiferenciaMonto",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCalculadoEfectivo",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCalculadoOtros",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCalculadoTarjetas",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCalculadoTotal",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCalculadoTransferencias",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDeclaradoEfectivo",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDeclaradoOtros",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDeclaradoTarjetas",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDeclaradoTotal",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDeclaradoTransferencias",
                table: "CajaSesiones",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesCierre",
                table: "CajaSesiones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PuntoEmisionId",
                table: "CajaSesiones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CajaSesiones",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "CajaMovimientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CajaSesionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Concepto = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    ComprobanteReferencia = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaMovimiento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajaMovimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CajaMovimientos_CajaSesiones_CajaSesionId",
                        column: x => x.CajaSesionId,
                        principalTable: "CajaSesiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturaPagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CajaSesionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormaPagoCodigo = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LoteNumero = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    VoucherNumero = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    BancoNombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    NumeroReferencia = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaPagos_CajaSesiones_CajaSesionId",
                        column: x => x.CajaSesionId,
                        principalTable: "CajaSesiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturaPagos_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CajaSesiones_BodegaId",
                table: "CajaSesiones",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_CajaSesiones_EmpresaId_PuntoEmisionId_UsuarioId_EstadoCaja",
                table: "CajaSesiones",
                columns: new[] { "EmpresaId", "PuntoEmisionId", "UsuarioId", "EstadoCaja" });

            migrationBuilder.CreateIndex(
                name: "IX_CajaSesiones_PuntoEmisionId",
                table: "CajaSesiones",
                column: "PuntoEmisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CajaMovimientos_CajaSesionId",
                table: "CajaMovimientos",
                column: "CajaSesionId");

            migrationBuilder.CreateIndex(
                name: "IX_CajaMovimientos_EmpresaId_CajaSesionId_FechaMovimiento",
                table: "CajaMovimientos",
                columns: new[] { "EmpresaId", "CajaSesionId", "FechaMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaPagos_CajaSesionId",
                table: "FacturaPagos",
                column: "CajaSesionId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaPagos_EmpresaId_CajaSesionId_FacturaId",
                table: "FacturaPagos",
                columns: new[] { "EmpresaId", "CajaSesionId", "FacturaId" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaPagos_FacturaId",
                table: "FacturaPagos",
                column: "FacturaId");

            migrationBuilder.Sql("""
                INSERT INTO FacturaPagos (Id, EmpresaId, FacturaId, CajaSesionId, FormaPagoCodigo, Monto, LoteNumero, VoucherNumero, BancoNombre, NumeroReferencia)
                SELECT NEWID(), EmpresaId, Id, CajaSesionId, FormaPagoSriCodigo, Total, NULL, NULL, NULL, NULL
                FROM Facturas
                WHERE CajaSesionId IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM FacturaPagos pagos
                      WHERE pagos.FacturaId = Facturas.Id
                  );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_CajaSesiones_Bodegas_BodegaId",
                table: "CajaSesiones",
                column: "BodegaId",
                principalTable: "Bodegas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CajaSesiones_EmpresaPuntosEmision_PuntoEmisionId",
                table: "CajaSesiones",
                column: "PuntoEmisionId",
                principalTable: "EmpresaPuntosEmision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CajaSesiones_Bodegas_BodegaId",
                table: "CajaSesiones");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaSesiones_EmpresaPuntosEmision_PuntoEmisionId",
                table: "CajaSesiones");

            migrationBuilder.DropTable(
                name: "CajaMovimientos");

            migrationBuilder.DropTable(
                name: "FacturaPagos");

            migrationBuilder.DropIndex(
                name: "IX_CajaSesiones_BodegaId",
                table: "CajaSesiones");

            migrationBuilder.DropIndex(
                name: "IX_CajaSesiones_EmpresaId_PuntoEmisionId_UsuarioId_EstadoCaja",
                table: "CajaSesiones");

            migrationBuilder.DropIndex(
                name: "IX_CajaSesiones_PuntoEmisionId",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "DiferenciaMonto",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoCalculadoEfectivo",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoCalculadoOtros",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoCalculadoTarjetas",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoCalculadoTotal",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoCalculadoTransferencias",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoDeclaradoEfectivo",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoDeclaradoOtros",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoDeclaradoTarjetas",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoDeclaradoTotal",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoDeclaradoTransferencias",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "ObservacionesCierre",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "PuntoEmisionId",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CajaSesiones");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVentasTransferenciaCalculado",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVentasTarjetaCalculado",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVentasEfectivoCalculado",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFisicoTransferenciaReal",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFisicoTarjetaReal",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFisicoEfectivoReal",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoApertura",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiferenciaTransferencia",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiferenciaTarjeta",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiferenciaEfectivo",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Diferencia",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);
        }
    }
}
