using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTesoreriaAndConciliacionBancaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuentasBancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BancoNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoCuenta = table.Column<byte>(type: "tinyint", nullable: false),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SaldoContable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SaldoConciliado = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Moneda = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "USD"),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtractoBancarioHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaImportacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TotalRegistros = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractoBancarioHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractoBancarioHeaders_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosTesoreria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Beneficiario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FacturaVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AsientoContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstadoConciliacion = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosTesoreria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_Facturas_FacturaVentaId",
                        column: x => x.FacturaVentaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtractoBancarioDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtractoHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaTransaccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroDocumentoRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConceptoDescripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoMovimiento = table.Column<byte>(type: "tinyint", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Conciliado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MovimientoTesoreriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractoBancarioDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractoBancarioDetalles_ExtractoBancarioHeaders_ExtractoHeaderId",
                        column: x => x.ExtractoHeaderId,
                        principalTable: "ExtractoBancarioHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtractoBancarioDetalles_MovimientosTesoreria_MovimientoTesoreriaId",
                        column: x => x.MovimientoTesoreriaId,
                        principalTable: "MovimientosTesoreria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_CuentaContableId",
                table: "CuentasBancarias",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_EmpresaId_Activa",
                table: "CuentasBancarias",
                columns: new[] { "EmpresaId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_EmpresaId_NumeroCuenta",
                table: "CuentasBancarias",
                columns: new[] { "EmpresaId", "NumeroCuenta" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_ExtractoHeaderId",
                table: "ExtractoBancarioDetalles",
                column: "ExtractoHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_ExtractoHeaderId_Conciliado",
                table: "ExtractoBancarioDetalles",
                columns: new[] { "ExtractoHeaderId", "Conciliado" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_FechaTransaccion",
                table: "ExtractoBancarioDetalles",
                column: "FechaTransaccion");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_FechaTransaccion_NumeroDocumentoRef",
                table: "ExtractoBancarioDetalles",
                columns: new[] { "FechaTransaccion", "NumeroDocumentoRef" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_MovimientoTesoreriaId",
                table: "ExtractoBancarioDetalles",
                column: "MovimientoTesoreriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_NumeroDocumentoRef",
                table: "ExtractoBancarioDetalles",
                column: "NumeroDocumentoRef");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioHeaders_CuentaBancariaId_FechaDesde_FechaHasta",
                table: "ExtractoBancarioHeaders",
                columns: new[] { "CuentaBancariaId", "FechaDesde", "FechaHasta" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioHeaders_FechaImportacion",
                table: "ExtractoBancarioHeaders",
                column: "FechaImportacion");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_AsientoContableId",
                table: "MovimientosTesoreria",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_CompraId",
                table: "MovimientosTesoreria",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_CuentaBancariaId_Fecha",
                table: "MovimientosTesoreria",
                columns: new[] { "CuentaBancariaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_EmpresaId_CuentaBancariaId_Fecha",
                table: "MovimientosTesoreria",
                columns: new[] { "EmpresaId", "CuentaBancariaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_EmpresaId_EstadoConciliacion_Fecha",
                table: "MovimientosTesoreria",
                columns: new[] { "EmpresaId", "EstadoConciliacion", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_FacturaVentaId",
                table: "MovimientosTesoreria",
                column: "FacturaVentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractoBancarioDetalles");

            migrationBuilder.DropTable(
                name: "ExtractoBancarioHeaders");

            migrationBuilder.DropTable(
                name: "MovimientosTesoreria");

            migrationBuilder.DropTable(
                name: "CuentasBancarias");
        }
    }
}
