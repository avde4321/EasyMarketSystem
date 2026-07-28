using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoubleEntryJournalStage102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsientosContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroAsiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaContable = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ModuloOrigen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentoSoporte = table.Column<string>(type: "nvarchar(49)", maxLength: 49, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosContables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsientosDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsientoContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Debe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsientosDetalle_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsientosDetalle_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_NumeroAsiento",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "NumeroAsiento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsientosDetalle_AsientoContableId",
                table: "AsientosDetalle",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosDetalle_CuentaContableId",
                table: "AsientosDetalle",
                column: "CuentaContableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsientosDetalle");

            migrationBuilder.DropTable(
                name: "AsientosContables");
        }
    }
}
