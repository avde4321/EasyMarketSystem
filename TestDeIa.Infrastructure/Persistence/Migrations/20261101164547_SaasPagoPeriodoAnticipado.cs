using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SaasPagoPeriodoAnticipado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeriodoMeses",
                table: "SaasPagoRegistros",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaasPagoRegistros_PeriodoMeses",
                table: "SaasPagoRegistros",
                sql: "[PeriodoMeses] BETWEEN 1 AND 12");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [SaasPagoRegistros] WHERE [PeriodoMeses] <> 1) THROW 51004, 'Rollback bloqueado: existen pagos anticipados con períodos mayores a un mes. Preserve esos datos antes de retirar la columna.', 1;");
            migrationBuilder.DropCheckConstraint(
                name: "CK_SaasPagoRegistros_PeriodoMeses",
                table: "SaasPagoRegistros");

            migrationBuilder.DropColumn(
                name: "PeriodoMeses",
                table: "SaasPagoRegistros");
        }
    }
}
