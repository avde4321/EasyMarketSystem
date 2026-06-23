using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonasModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonaId",
                table: "SecurityUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonaId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Personas",
                columns: new[] { "Id", "Apellidos", "CreatedAt", "Direccion", "Email", "FechaNacimiento", "Identificacion", "IsActive", "Nombres", "Telefono", "TipoIdentificacion", "UpdatedAt" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "Sistema", new DateTimeOffset(new DateTime(2026, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "admin@testdeia.local", null, "ADMIN", true, "Administrador", null, "Sistema", null });

            migrationBuilder.UpdateData(
                table: "SecurityUsers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PersonaId",
                value: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_PersonaId",
                table: "SecurityUsers",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Identificacion",
                table: "Personas",
                column: "Identificacion",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Personas_PersonaId",
                table: "Clientes",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityUsers_Personas_PersonaId",
                table: "SecurityUsers",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Personas_PersonaId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityUsers_Personas_PersonaId",
                table: "SecurityUsers");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_PersonaId",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PersonaId",
                table: "SecurityUsers");

            migrationBuilder.DropColumn(
                name: "PersonaId",
                table: "Clientes");
        }
    }
}
