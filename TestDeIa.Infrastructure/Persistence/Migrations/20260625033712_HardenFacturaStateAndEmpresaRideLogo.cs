using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenFacturaStateAndEmpresaRideLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "LogoRideContenido",
                table: "EmpresasEmisoras",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoRideMimeType",
                table: "EmpresasEmisoras",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmpresasEmisoras",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "LogoRideContenido", "LogoRideMimeType" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoRideContenido",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "LogoRideMimeType",
                table: "EmpresasEmisoras");
        }
    }
}
