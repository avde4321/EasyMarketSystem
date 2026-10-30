using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarNumeracionComprobantesPorAmbiente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_CodigoDocumento_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobantesRetencion");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_TipoDocumentoId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras");

            migrationBuilder.AddColumn<string>(
                name: "AmbienteSri",
                table: "FacturaSecuenciales",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "1");

            migrationBuilder.AddColumn<string>(
                name: "AmbienteSri",
                table: "Compras",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "1");

            // Conservar el contador legado en el ambiente actual de la empresa.
            // Los máximos históricos de cada ambiente se verifican además al reservar.
            migrationBuilder.Sql("""
                UPDATE s SET AmbienteSri = CASE WHEN e.AmbienteSri IN ('2', 'Produccion', 'Producción') THEN '2' ELSE '1' END
                FROM FacturaSecuenciales s INNER JOIN EmpresasEmisoras e ON e.Id = s.EmpresaId;
                UPDATE Compras SET AmbienteSri = SUBSTRING(ClaveAccesoGenerada, 24, 1)
                WHERE LEN(ClaveAccesoGenerada) = 49 AND SUBSTRING(ClaveAccesoGenerada, 24, 1) IN ('1','2');
                """);

            migrationBuilder.CreateTable(
                name: "NumeracionHistorial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    AmbienteSri = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CodigoDocumento = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Anterior = table.Column<long>(type: "bigint", nullable: false),
                    Nuevo = table.Column<long>(type: "bigint", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumeracionHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumeracionHistorial_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_AmbienteSri_CodigoDocumento_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales",
                columns: new[] { "EmpresaId", "AmbienteSri", "CodigoDocumento", "Establecimiento", "PuntoEmision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_AmbienteSri_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas",
                columns: new[] { "EmpresaId", "AmbienteSri", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_AmbienteSRI_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "AmbienteSRI", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_AmbienteSri_TipoDocumentoId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "AmbienteSri", "TipoDocumentoId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_AmbienteSri_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras",
                columns: new[] { "EmpresaId", "AmbienteSri", "TipoDocumentoCodigo", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionHistorial_EmpresaId_Fecha",
                table: "NumeracionHistorial",
                columns: new[] { "EmpresaId", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM NumeracionHistorial)
                    THROW 51000, 'Rollback bloqueado: existen cambios de numeracion auditados. Requiere revision del DBA.', 1;
                IF EXISTS (SELECT 1 FROM FacturaSecuenciales GROUP BY EmpresaId, CodigoDocumento, Establecimiento, PuntoEmision HAVING COUNT(*) > 1)
                   OR EXISTS (SELECT 1 FROM Facturas GROUP BY EmpresaId, Establecimiento, PuntoEmision, Secuencial HAVING COUNT(*) > 1)
                   OR EXISTS (SELECT 1 FROM ComprobanteCabecera GROUP BY EmpresaId, TipoDocumentoId, Establecimiento, PuntoEmision, Secuencial HAVING COUNT(*) > 1)
                   OR EXISTS (SELECT 1 FROM ComprobantesRetencion GROUP BY EmpresaId, Establecimiento, PuntoEmision, Secuencial HAVING COUNT(*) > 1)
                   OR EXISTS (SELECT 1 FROM Compras GROUP BY EmpresaId, TipoDocumentoCodigo, Establecimiento, PuntoEmision, Secuencial HAVING COUNT(*) > 1)
                    THROW 51000, 'Rollback bloqueado: hay numeraciones independientes por ambiente que no se pueden fusionar.', 1;
                """);
            migrationBuilder.DropTable(
                name: "NumeracionHistorial");

            migrationBuilder.DropIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_AmbienteSri_CodigoDocumento_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_AmbienteSri_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_AmbienteSRI_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobantesRetencion");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_AmbienteSri_TipoDocumentoId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_AmbienteSri_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "AmbienteSri",
                table: "FacturaSecuenciales");

            migrationBuilder.DropColumn(
                name: "AmbienteSri",
                table: "Compras");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_CodigoDocumento_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales",
                columns: new[] { "EmpresaId", "CodigoDocumento", "Establecimiento", "PuntoEmision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_TipoDocumentoId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "TipoDocumentoId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras",
                columns: new[] { "EmpresaId", "TipoDocumentoCodigo", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);
        }
    }
}
