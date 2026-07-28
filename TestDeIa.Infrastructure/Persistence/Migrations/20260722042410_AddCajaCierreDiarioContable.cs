using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCajaCierreDiarioContable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AsientoContableId",
                table: "CajaSesiones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Diferencia",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiferenciaTransferencia",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFisicoTransferenciaReal",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVentasTransferenciaCalculado",
                table: "CajaSesiones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_CajaSesiones_AsientoContableId",
                table: "CajaSesiones",
                column: "AsientoContableId");

            migrationBuilder.AddForeignKey(
                name: "FK_CajaSesiones_AsientosContables_AsientoContableId",
                table: "CajaSesiones",
                column: "AsientoContableId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CajaSesiones_AsientosContables_AsientoContableId",
                table: "CajaSesiones");

            migrationBuilder.DropIndex(
                name: "IX_CajaSesiones_AsientoContableId",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "AsientoContableId",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "Diferencia",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "DiferenciaTransferencia",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "MontoFisicoTransferenciaReal",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "TotalVentasTransferenciaCalculado",
                table: "CajaSesiones");
        }
    }
}
