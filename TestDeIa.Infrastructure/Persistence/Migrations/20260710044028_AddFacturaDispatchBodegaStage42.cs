using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaDispatchBodegaStage42 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BodegaId",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE factura
                SET factura.BodegaId = bodega.Id
                FROM Facturas factura
                INNER JOIN Bodegas bodega
                    ON bodega.EmpresaId = factura.EmpresaId
                   AND bodega.Nombre = N'Principal'
                WHERE factura.BodegaId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "BodegaId",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_BodegaId",
                table: "Facturas",
                column: "BodegaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Bodegas_BodegaId",
                table: "Facturas",
                column: "BodegaId",
                principalTable: "Bodegas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Bodegas_BodegaId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_BodegaId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                table: "Facturas");
        }
    }
}
