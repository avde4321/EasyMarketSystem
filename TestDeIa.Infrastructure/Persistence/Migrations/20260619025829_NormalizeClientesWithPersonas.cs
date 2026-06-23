using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeClientesWithPersonas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [Personas] (
                    [Id],
                    [TipoIdentificacion],
                    [Identificacion],
                    [Nombres],
                    [Apellidos],
                    [FechaNacimiento],
                    [Email],
                    [Telefono],
                    [Direccion],
                    [IsActive],
                    [CreatedAt],
                    [UpdatedAt])
                SELECT
                    NEWID(),
                    N'Cedula',
                    [c].[Identificacion],
                    [c].[Nombres],
                    [c].[Apellidos],
                    NULL,
                    [c].[Email],
                    [c].[Telefono],
                    [c].[Direccion],
                    [c].[IsActive],
                    [c].[CreatedAt],
                    [c].[UpdatedAt]
                FROM [Clientes] [c]
                WHERE [c].[PersonaId] IS NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [Personas] [p]
                      WHERE [p].[Identificacion] = [c].[Identificacion]);
                """);

            migrationBuilder.Sql("""
                UPDATE [c]
                SET [c].[PersonaId] = [p].[Id]
                FROM [Clientes] [c]
                INNER JOIN [Personas] [p] ON [p].[Identificacion] = [c].[Identificacion]
                WHERE [c].[PersonaId] IS NULL;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Identificacion",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Nombres",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Clientes");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonaId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes",
                column: "PersonaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonaId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Clientes",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombres",
                table: "Clientes",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Identificacion",
                table: "Clientes",
                column: "Identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes",
                column: "PersonaId");
        }
    }
}
