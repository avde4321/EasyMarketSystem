using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SaasCuposFacturaAtomicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaasFacturaCupos",
                columns: table => new
                {
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    FechaReserva = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaConfirmacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PeriodoAutorizacion = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasFacturaCupos", x => x.FacturaId);
                    table.CheckConstraint("CK_SaasFacturaCupos_Estado", "[Estado] IN ('RESERVADO','CONFIRMADO','LIBERADO')");
                    table.ForeignKey(
                        name: "FK_SaasFacturaCupos_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaasFacturaCupos_EmpresaId_Estado_PeriodoAutorizacion",
                table: "SaasFacturaCupos",
                columns: new[] { "EmpresaId", "Estado", "PeriodoAutorizacion" });

            // Reconstruir consumo real; el contador anterior se incrementaba durante los intentos.
            migrationBuilder.Sql("""
                INSERT INTO SaasFacturaCupos (FacturaId, EmpresaId, Estado, FechaReserva, FechaConfirmacion, PeriodoAutorizacion)
                SELECT f.Id, f.EmpresaId,
                       CASE WHEN f.Estado = 'AUTORIZADO' THEN 'CONFIRMADO' ELSE 'RESERVADO' END,
                       f.CreatedAt,
                       CASE WHEN f.Estado = 'AUTORIZADO' THEN COALESCE(f.FechaAutorizacion, f.CreatedAt) ELSE NULL END,
                       CASE WHEN f.Estado = 'AUTORIZADO' THEN DATEFROMPARTS(
                           YEAR(SWITCHOFFSET(COALESCE(f.FechaAutorizacion, f.CreatedAt), '+00:00')),
                           MONTH(SWITCHOFFSET(COALESCE(f.FechaAutorizacion, f.CreatedAt), '+00:00')), 1) ELSE NULL END
                FROM Facturas f
                WHERE f.Estado IN ('AUTORIZADO', 'NO_FIRMADO', 'PENDIENTE');

                DECLARE @period date = DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1);
                UPDATE s SET FacturasEmitidasMesActual = (
                    SELECT COUNT(*) FROM SaasFacturaCupos c
                    WHERE c.EmpresaId = s.EmpresaId AND c.Estado = 'CONFIRMADO' AND c.PeriodoAutorizacion = @period),
                    ContadorFacturasPeriodo = @period
                FROM SaasSuscripciones s;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM SaasFacturaCupos)
                    THROW 51000, 'Rollback bloqueado: hay reservas o consumos SaaS. Requiere reconciliacion del DBA.', 1;
                """);
            migrationBuilder.DropTable(
                name: "SaasFacturaCupos");
        }
    }
}
