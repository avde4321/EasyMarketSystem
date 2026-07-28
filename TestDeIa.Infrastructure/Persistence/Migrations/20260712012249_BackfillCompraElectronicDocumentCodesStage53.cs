using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillCompraElectronicDocumentCodesStage53 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE FacturaSecuenciales
                SET CodigoDocumento = '01'
                WHERE CodigoDocumento = '' OR CodigoDocumento IS NULL;

                UPDATE Compras
                SET TipoDocumentoCodigo = '01'
                WHERE TipoDocumentoCodigo = '' OR TipoDocumentoCodigo IS NULL;

                UPDATE Compras
                SET FormaPagoSriCodigo = '01'
                WHERE FormaPagoSriCodigo = '' OR FormaPagoSriCodigo IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
