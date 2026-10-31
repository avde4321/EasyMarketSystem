using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMaintenanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoReferencial",
                table: "Productos",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NaturalezaItem",
                table: "Productos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Mercaderia");

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "Productos",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Unidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CostoReferencial",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "NaturalezaItem",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "Productos");
        }
    }
}
