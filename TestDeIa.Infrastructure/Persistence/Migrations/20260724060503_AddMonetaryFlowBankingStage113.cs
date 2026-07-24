using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMonetaryFlowBankingStage113 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CuentaContableSalidaId",
                table: "PagosCxP",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroComprobantePago",
                table: "PagosCxP",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagoCompra",
                table: "Compras",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "ContadoEfectivo");

            migrationBuilder.AddColumn<bool>(
                name: "RequiereBancarizacion",
                table: "Compras",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PagosCxP_CuentaContableSalidaId",
                table: "PagosCxP",
                column: "CuentaContableSalidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PagosCxP_CuentasContables_CuentaContableSalidaId",
                table: "PagosCxP",
                column: "CuentaContableSalidaId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagosCxP_CuentasContables_CuentaContableSalidaId",
                table: "PagosCxP");

            migrationBuilder.DropIndex(
                name: "IX_PagosCxP_CuentaContableSalidaId",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "CuentaContableSalidaId",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "NumeroComprobantePago",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "FormaPagoCompra",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "RequiereBancarizacion",
                table: "Compras");
        }
    }
}
