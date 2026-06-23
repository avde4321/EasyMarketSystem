using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefineSriInvoiceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClienteNombre",
                table: "Facturas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "ClienteDireccion",
                table: "Facturas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteEmail",
                table: "Facturas",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteTelefono",
                table: "Facturas",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteTipoIdentificacion",
                table: "Facturas",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FormaPagoSriCodigo",
                table: "Facturas",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDescuento",
                table: "Facturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE facturas
                SET facturas.ClienteTipoIdentificacion =
                    CASE UPPER(LTRIM(RTRIM(personas.TipoIdentificacion)))
                        WHEN 'RUC' THEN '04'
                        WHEN 'CEDULA' THEN '05'
                        WHEN 'CÉDULA' THEN '05'
                        WHEN 'PASAPORTE' THEN '06'
                        WHEN 'CONSUMIDOR FINAL' THEN '07'
                        WHEN 'IDENTIFICACION DEL EXTERIOR' THEN '08'
                        WHEN 'IDENTIFICACIÓN DEL EXTERIOR' THEN '08'
                        WHEN 'PLACA' THEN '09'
                        ELSE '05'
                    END
                FROM Facturas facturas
                INNER JOIN Clientes clientes ON clientes.Id = facturas.ClienteId
                INNER JOIN Personas personas ON personas.Id = clientes.PersonaId
                WHERE facturas.ClienteTipoIdentificacion = '';
                """);

            migrationBuilder.Sql(
                """
                UPDATE Facturas
                SET FormaPagoSriCodigo =
                    CASE UPPER(LTRIM(RTRIM(FormaPago)))
                        WHEN 'EFECTIVO' THEN '01'
                        WHEN 'COMPENSACION' THEN '15'
                        WHEN 'COMPENSACIÓN' THEN '15'
                        WHEN 'TARJETA DE DEBITO' THEN '16'
                        WHEN 'TARJETA DE DÉBITO' THEN '16'
                        WHEN 'DINERO ELECTRONICO' THEN '17'
                        WHEN 'DINERO ELECTRÓNICO' THEN '17'
                        WHEN 'TARJETA PREPAGO' THEN '18'
                        WHEN 'TARJETA' THEN '19'
                        WHEN 'TARJETA DE CREDITO' THEN '19'
                        WHEN 'TARJETA DE CRÉDITO' THEN '19'
                        WHEN 'TRANSFERENCIA' THEN '20'
                        WHEN 'OTROS CON UTILIZACION DEL SISTEMA FINANCIERO' THEN '20'
                        WHEN 'OTROS CON UTILIZACIÓN DEL SISTEMA FINANCIERO' THEN '20'
                        WHEN 'ENDOSO DE TITULOS' THEN '21'
                        WHEN 'ENDOSO DE TÍTULOS' THEN '21'
                        ELSE '01'
                    END
                WHERE FormaPagoSriCodigo = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClienteDireccion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ClienteEmail",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ClienteTelefono",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ClienteTipoIdentificacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "FormaPagoSriCodigo",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TotalDescuento",
                table: "Facturas");

            migrationBuilder.AlterColumn<string>(
                name: "ClienteNombre",
                table: "Facturas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);
        }
    }
}
