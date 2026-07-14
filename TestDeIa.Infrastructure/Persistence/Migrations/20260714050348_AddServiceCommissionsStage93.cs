using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCommissionsStage93 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaComision",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TipoComision",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorComision",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoComisionCalculado",
                table: "FacturaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioIdOperador",
                table: "FacturaDetalles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturaDetalles_UsuarioIdOperador",
                table: "FacturaDetalles",
                column: "UsuarioIdOperador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturaDetalles_UsuarioIdOperador",
                table: "FacturaDetalles");

            migrationBuilder.DropColumn(
                name: "AplicaComision",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TipoComision",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ValorComision",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MontoComisionCalculado",
                table: "FacturaDetalles");

            migrationBuilder.DropColumn(
                name: "UsuarioIdOperador",
                table: "FacturaDetalles");
        }
    }
}
