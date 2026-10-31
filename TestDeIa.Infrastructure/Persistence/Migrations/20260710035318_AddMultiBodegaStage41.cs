using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiBodegaStage41 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_ProductoId_FechaMovimiento",
                table: "KardexMovimientos");

            migrationBuilder.AddColumn<Guid>(
                name: "BodegaId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bodegas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bodegas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductosBodega",
                columns: table => new
                {
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockActual = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosBodega", x => new { x.ProductoId, x.BodegaId });
                    table.ForeignKey(
                        name: "FK_ProductosBodega_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosBodega_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO Bodegas (Id, EmpresaId, Nombre, Direccion, IsActive, CreatedAt, UpdatedAt)
                SELECT NEWID(), source.EmpresaId, N'Principal', N'Matriz principal', CAST(1 AS bit), SYSDATETIMEOFFSET(), NULL
                FROM (
                    SELECT DISTINCT EmpresaId
                    FROM Productos
                ) AS source
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM Bodegas currentBodega
                    WHERE currentBodega.EmpresaId = source.EmpresaId
                      AND currentBodega.Nombre = N'Principal'
                );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO ProductosBodega (ProductoId, BodegaId, EmpresaId, StockActual)
                SELECT producto.Id,
                       bodega.Id,
                       producto.EmpresaId,
                       producto.StockActual
                FROM Productos producto
                INNER JOIN Bodegas bodega
                    ON bodega.EmpresaId = producto.EmpresaId
                   AND bodega.Nombre = N'Principal'
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ProductosBodega productoBodega
                    WHERE productoBodega.ProductoId = producto.Id
                      AND productoBodega.BodegaId = bodega.Id
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE kardex
                SET kardex.BodegaId = bodega.Id
                FROM KardexMovimientos kardex
                INNER JOIN Productos producto
                    ON producto.Id = kardex.ProductoId
                INNER JOIN Bodegas bodega
                    ON bodega.EmpresaId = producto.EmpresaId
                   AND bodega.Nombre = N'Principal'
                WHERE kardex.BodegaId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "BodegaId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockActual",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_BodegaId",
                table: "KardexMovimientos",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_ProductoId_BodegaId_FechaMovimiento",
                table: "KardexMovimientos",
                columns: new[] { "ProductoId", "BodegaId", "FechaMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Bodegas_EmpresaId_Nombre",
                table: "Bodegas",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosBodega_BodegaId",
                table: "ProductosBodega",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosBodega_EmpresaId_BodegaId",
                table: "ProductosBodega",
                columns: new[] { "EmpresaId", "BodegaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_KardexMovimientos_Bodegas_BodegaId",
                table: "KardexMovimientos",
                column: "BodegaId",
                principalTable: "Bodegas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KardexMovimientos_Bodegas_BodegaId",
                table: "KardexMovimientos");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Productos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "StockActual",
                table: "Productos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE producto
                SET producto.StockActual = ISNULL(stock.TotalStock, 0)
                FROM Productos producto
                OUTER APPLY (
                    SELECT SUM(productoBodega.StockActual) AS TotalStock
                    FROM ProductosBodega productoBodega
                    WHERE productoBodega.ProductoId = producto.Id
                ) stock;
                """);

            migrationBuilder.DropTable(
                name: "ProductosBodega");

            migrationBuilder.DropTable(
                name: "Bodegas");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_BodegaId",
                table: "KardexMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_KardexMovimientos_ProductoId_BodegaId_FechaMovimiento",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                table: "KardexMovimientos");

            migrationBuilder.CreateIndex(
                name: "IX_KardexMovimientos_ProductoId_FechaMovimiento",
                table: "KardexMovimientos",
                columns: new[] { "ProductoId", "FechaMovimiento" });
        }
    }
}
