using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompraFiscalClassificationStage111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NaturalezaCompra",
                table: "Compras",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SustentoTributarioSRI",
                table: "Compras",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "01");

            migrationBuilder.AddColumn<string>(
                name: "TipoComprobanteSRI",
                table: "Compras",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "01");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductoId",
                table: "CompraDetalles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CategoriaSriActivo",
                table: "CompraDetalles",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaturalezaCompra",
                table: "CompraDetalles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NombreActivo",
                table: "CompraDetalles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerieUbicacionActivo",
                table: "CompraDetalles",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NaturalezaCompra",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "SustentoTributarioSRI",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "TipoComprobanteSRI",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "CategoriaSriActivo",
                table: "CompraDetalles");

            migrationBuilder.DropColumn(
                name: "NaturalezaCompra",
                table: "CompraDetalles");

            migrationBuilder.DropColumn(
                name: "NombreActivo",
                table: "CompraDetalles");

            migrationBuilder.DropColumn(
                name: "SerieUbicacionActivo",
                table: "CompraDetalles");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductoId",
                table: "CompraDetalles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
