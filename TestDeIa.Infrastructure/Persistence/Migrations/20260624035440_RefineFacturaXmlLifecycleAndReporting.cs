using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefineFacturaXmlLifecycleAndReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "XmlGenerado",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                ;WITH LegacyFacturas AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS RowNumber
                    FROM Facturas
                    WHERE ClaveAcceso IS NULL OR LTRIM(RTRIM(ClaveAcceso)) = ''
                )
                UPDATE Facturas
                SET ClaveAcceso = RIGHT(REPLICATE('0', 49) + CAST(LegacyFacturas.RowNumber AS varchar(49)), 49)
                FROM Facturas
                INNER JOIN LegacyFacturas ON LegacyFacturas.Id = Facturas.Id;

                UPDATE Facturas
                SET XmlGenerado = COALESCE(XmlGenerado, XmlFirmado)
                WHERE XmlGenerado IS NULL AND XmlFirmado IS NOT NULL;

                UPDATE Facturas
                SET Estado = CASE
                    WHEN UPPER(Estado) = 'AUTORIZADO' THEN 'AUTORIZADO'
                    WHEN UPPER(Estado) = 'RECHAZADO' THEN 'RECHAZADO'
                    WHEN XmlFirmado IS NOT NULL THEN 'PENDIENTE'
                    ELSE 'NO_FIRMADO'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ClaveAcceso",
                table: "Facturas",
                type: "nvarchar(49)",
                maxLength: 49,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ClaveAcceso",
                table: "Facturas",
                column: "ClaveAcceso",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_ClaveAcceso",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlGenerado",
                table: "Facturas");

            migrationBuilder.AlterColumn<string>(
                name: "ClaveAcceso",
                table: "Facturas",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(49)",
                oldMaxLength: 49);
        }
    }
}
