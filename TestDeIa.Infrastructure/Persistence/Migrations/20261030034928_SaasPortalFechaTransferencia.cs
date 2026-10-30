using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SaasPortalFechaTransferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaPagoInformada",
                table: "SaasPagoRegistros",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM SaasPagoRegistros WHERE FechaPagoInformada IS NOT NULL)
                    THROW 51003, 'Rollback bloqueado: hay fechas de pago informadas. Preserve los datos antes de retirar la columna.', 1;
                """);
            migrationBuilder.DropColumn(
                name: "FechaPagoInformada",
                table: "SaasPagoRegistros");
        }
    }
}
