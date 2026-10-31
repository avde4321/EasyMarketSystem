using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpleadoLaborFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cargo",
                table: "Empleados",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaIngreso",
                table: "Empleados",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerfilLaboral",
                table: "Empleados",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cargo",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaIngreso",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "PerfilLaboral",
                table: "Empleados");
        }
    }
}
