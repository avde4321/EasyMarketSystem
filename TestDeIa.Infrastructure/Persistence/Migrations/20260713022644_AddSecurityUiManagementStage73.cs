using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityUiManagementStage73 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BloqueadoManualmente",
                table: "SecurityUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TokensInvalidosDesde",
                table: "SecurityUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SecurityUsers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "TokensInvalidosDesde",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloqueadoManualmente",
                table: "SecurityUsers");

            migrationBuilder.DropColumn(
                name: "TokensInvalidosDesde",
                table: "SecurityUsers");
        }
    }
}
