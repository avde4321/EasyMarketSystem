using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityHardeningStage71 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "SecurityUsers",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BloqueadoHasta",
                table: "SecurityUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "SecurityUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UltimoAcceso",
                table: "SecurityUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SecurityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEvento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DireccionIP = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Detalles = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "SecurityUsers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "BloqueadoHasta", "UltimoAcceso" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_EmpresaId_FechaEvento",
                table: "SecurityAuditLogs",
                columns: new[] { "EmpresaId", "FechaEvento" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UsuarioId_FechaEvento",
                table: "SecurityAuditLogs",
                columns: new[] { "UsuarioId", "FechaEvento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityAuditLogs");

            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "SecurityUsers");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "SecurityUsers");

            migrationBuilder.DropColumn(
                name: "UltimoAcceso",
                table: "SecurityUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "SecurityUsers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);
        }
    }
}
