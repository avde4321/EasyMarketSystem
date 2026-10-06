using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_TenantStorage_CifradoDocumental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlgoritmoCifrado",
                table: "DocumentosAdjuntos",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsCifrado",
                table: "DocumentosAdjuntos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlgoritmoCifrado",
                table: "DocumentosAdjuntos");

            migrationBuilder.DropColumn(
                name: "EsCifrado",
                table: "DocumentosAdjuntos");
        }
    }
}
