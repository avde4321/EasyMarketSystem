using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseTransferManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Bodegas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "001");

            migrationBuilder.AddColumn<bool>(
                name: "EsPrincipal",
                table: "Bodegas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                WITH BodegasOrdenadas AS
                (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY EmpresaId
                            ORDER BY
                                CASE WHEN Nombre = 'Principal' THEN 0 ELSE 1 END,
                                Nombre,
                                Id) AS Numero
                    FROM Bodegas
                )
                UPDATE b
                SET
                    Codigo = RIGHT('000' + CAST(o.Numero AS varchar(3)), 3),
                    EsPrincipal = CASE WHEN o.Numero = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
                FROM Bodegas b
                INNER JOIN BodegasOrdenadas o ON o.Id = b.Id;
                """);

            migrationBuilder.CreateTable(
                name: "TransferenciasInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEmision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaTraslado = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivoTraslado = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    GuiaRemisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferenciasInventario_Bodegas_BodegaDestinoId",
                        column: x => x.BodegaDestinoId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasInventario_Bodegas_BodegaOrigenId",
                        column: x => x.BodegaOrigenId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferenciasInventarioDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferenciaInventarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadEnviada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasInventarioDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferenciasInventarioDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasInventarioDetalle_TransferenciasInventario_TransferenciaInventarioId",
                        column: x => x.TransferenciaInventarioId,
                        principalTable: "TransferenciasInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bodegas_EmpresaId_Codigo",
                table: "Bodegas",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasInventario_BodegaDestinoId",
                table: "TransferenciasInventario",
                column: "BodegaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasInventario_BodegaOrigenId",
                table: "TransferenciasInventario",
                column: "BodegaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasInventario_EmpresaId_BodegaOrigenId_BodegaDestinoId",
                table: "TransferenciasInventario",
                columns: new[] { "EmpresaId", "BodegaOrigenId", "BodegaDestinoId" });

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasInventario_EmpresaId_Estado",
                table: "TransferenciasInventario",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasInventarioDetalle_ProductoId",
                table: "TransferenciasInventarioDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasInventarioDetalle_TransferenciaInventarioId_ProductoId",
                table: "TransferenciasInventarioDetalle",
                columns: new[] { "TransferenciaInventarioId", "ProductoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferenciasInventarioDetalle");

            migrationBuilder.DropTable(
                name: "TransferenciasInventario");

            migrationBuilder.DropIndex(
                name: "IX_Bodegas_EmpresaId_Codigo",
                table: "Bodegas");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Bodegas");

            migrationBuilder.DropColumn(
                name: "EsPrincipal",
                table: "Bodegas");
        }
    }
}
