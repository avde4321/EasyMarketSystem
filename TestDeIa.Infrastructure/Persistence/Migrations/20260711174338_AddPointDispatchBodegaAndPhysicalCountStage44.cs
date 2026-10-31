using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPointDispatchBodegaAndPhysicalCountStage44 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BodegaId",
                table: "EmpresaPuntosEmision",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                """
                INSERT INTO Bodegas (Id, EmpresaId, Nombre, Direccion, IsActive, CreatedAt, UpdatedAt)
                SELECT NEWID(), empresa.Id, N'Principal', NULL, CAST(1 AS bit), SYSUTCDATETIME(), NULL
                FROM EmpresasEmisoras empresa
                WHERE EXISTS (
                    SELECT 1
                    FROM EmpresaPuntosEmision punto
                    WHERE punto.EmpresaEmisoraId = empresa.Id)
                  AND NOT EXISTS (
                    SELECT 1
                    FROM Bodegas bodega
                    WHERE bodega.EmpresaId = empresa.Id);
                """);

            migrationBuilder.Sql(
                """
                ;WITH BodegaPreferida AS (
                    SELECT
                        punto.Id AS PuntoId,
                        COALESCE(
                            (
                                SELECT TOP (1) bodega.Id
                                FROM Bodegas bodega
                                WHERE bodega.EmpresaId = punto.EmpresaEmisoraId
                                  AND bodega.IsActive = 1
                                  AND bodega.Nombre = N'Principal'
                                ORDER BY bodega.Nombre
                            ),
                            (
                                SELECT TOP (1) bodega.Id
                                FROM Bodegas bodega
                                WHERE bodega.EmpresaId = punto.EmpresaEmisoraId
                                  AND bodega.IsActive = 1
                                ORDER BY bodega.Nombre
                            )
                        ) AS BodegaId
                    FROM EmpresaPuntosEmision punto
                )
                UPDATE punto
                SET punto.BodegaId = bodega.BodegaId
                FROM EmpresaPuntosEmision punto
                INNER JOIN BodegaPreferida bodega ON bodega.PuntoId = punto.Id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaPuntosEmision_BodegaId",
                table: "EmpresaPuntosEmision",
                column: "BodegaId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmpresaPuntosEmision_Bodegas_BodegaId",
                table: "EmpresaPuntosEmision",
                column: "BodegaId",
                principalTable: "Bodegas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmpresaPuntosEmision_Bodegas_BodegaId",
                table: "EmpresaPuntosEmision");

            migrationBuilder.DropIndex(
                name: "IX_EmpresaPuntosEmision_BodegaId",
                table: "EmpresaPuntosEmision");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                table: "EmpresaPuntosEmision");
        }
    }
}
