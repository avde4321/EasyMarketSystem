using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuentasPorPagarStage54 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuentasPorPagar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEmision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaVence = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MontoOriginal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstadoDeuda = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorPagar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosCxP",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaPorPagarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaPago = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ReferenciaTransaccion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosCxP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosCxP_CuentasPorPagar_CuentaPorPagarId",
                        column: x => x.CuentaPorPagarId,
                        principalTable: "CuentasPorPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_CompraId",
                table: "CuentasPorPagar",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_EmpresaId_ProveedorId_FechaVence",
                table: "CuentasPorPagar",
                columns: new[] { "EmpresaId", "ProveedorId", "FechaVence" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_ProveedorId",
                table: "CuentasPorPagar",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCxP_CuentaPorPagarId_FechaPago",
                table: "PagosCxP",
                columns: new[] { "CuentaPorPagarId", "FechaPago" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosCxP");

            migrationBuilder.DropTable(
                name: "CuentasPorPagar");
        }
    }
}
