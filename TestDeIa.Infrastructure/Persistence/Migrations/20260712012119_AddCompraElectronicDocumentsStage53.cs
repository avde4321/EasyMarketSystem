using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompraElectronicDocumentsStage53 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras");

            migrationBuilder.AddColumn<string>(
                name: "CodigoDocumento",
                table: "FacturaSecuenciales",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaveAccesoGenerada",
                table: "Compras",
                type: "nvarchar(49)",
                maxLength: 49,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoSri",
                table: "Compras",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagoSriCodigo",
                table: "Compras",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MensajeEstado",
                table: "Compras",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextRetryAt",
                table: "Compras",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroAutorizacion",
                table: "Compras",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "Compras",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingNode",
                table: "Compras",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingStartedAt",
                table: "Compras",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Compras",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumentoCodigo",
                table: "Compras",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "XmlFirmado",
                table: "Compras",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlGenerado",
                table: "Compras",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE FacturaSecuenciales
                SET CodigoDocumento = '01'
                WHERE CodigoDocumento = '';

                UPDATE Compras
                SET TipoDocumentoCodigo = '01'
                WHERE TipoDocumentoCodigo = '';

                UPDATE Compras
                SET FormaPagoSriCodigo = '01'
                WHERE FormaPagoSriCodigo = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_CodigoDocumento_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales",
                columns: new[] { "EmpresaId", "CodigoDocumento", "Establecimiento", "PuntoEmision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras",
                columns: new[] { "EmpresaId", "TipoDocumentoCodigo", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_CodigoDocumento_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "CodigoDocumento",
                table: "FacturaSecuenciales");

            migrationBuilder.DropColumn(
                name: "ClaveAccesoGenerada",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "EstadoSri",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "FormaPagoSriCodigo",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "MensajeEstado",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "NumeroAutorizacion",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "ProcessingNode",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAt",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoCodigo",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "XmlFirmado",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "XmlGenerado",
                table: "Compras");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaSecuenciales_EmpresaId_Establecimiento_PuntoEmision",
                table: "FacturaSecuenciales",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "Compras",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);
        }
    }
}
