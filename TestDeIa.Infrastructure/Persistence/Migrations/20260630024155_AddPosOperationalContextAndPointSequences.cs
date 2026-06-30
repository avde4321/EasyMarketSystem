using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPosOperationalContextAndPointSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacturaSecuenciales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PuntoEmision = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    UltimoSecuencial = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaSecuenciales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO FacturaSecuenciales (Id, EmpresaId, Establecimiento, PuntoEmision, UltimoSecuencial, CreatedAt, UpdatedAt)
                SELECT NEWID(), EmpresaId, Establecimiento, PuntoEmision, MAX(Secuencial), SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM Facturas
                GROUP BY EmpresaId, Establecimiento, PuntoEmision;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Facturas_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas");

            migrationBuilder.AddColumn<long>(
                name: "SecuencialTmp",
                table: "Facturas",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Facturas
                SET SecuencialTmp = Secuencial;
                """);

            migrationBuilder.DropColumn(
                name: "Secuencial",
                table: "Facturas");

            migrationBuilder.RenameColumn(
                name: "SecuencialTmp",
                table: "Facturas",
                newName: "Secuencial");

            migrationBuilder.AlterColumn<long>(
                name: "Secuencial",
                table: "Facturas",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas",
                columns: new[] { "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturaSecuenciales");

            migrationBuilder.AlterColumn<long>(
                name: "Secuencial",
                table: "Facturas",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
