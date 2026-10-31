using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaCashSettlementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MontoRecibido",
                table: "Facturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VueltoEntregado",
                table: "Facturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoRecibido",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "VueltoEntregado",
                table: "Facturas");
        }
    }
}
