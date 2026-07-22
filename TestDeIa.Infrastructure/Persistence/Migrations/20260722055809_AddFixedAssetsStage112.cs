using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedAssetsStage112 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivosFijos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompraDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CodigoActivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    SerieMarca = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CategoriaSRI = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorResidual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VidaUtilAnios = table.Column<int>(type: "int", nullable: false),
                    PorcentajeDepreciacionAnual = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    UbicacionFisica = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CustodioResponsable = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    EstadoActivo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosFijos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_CompraDetalles_CompraDetalleId",
                        column: x => x.CompraDetalleId,
                        principalTable: "CompraDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_CompraDetalleId",
                table: "ActivosFijos",
                column: "CompraDetalleId",
                unique: true,
                filter: "[CompraDetalleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_EmpresaId_CodigoActivo",
                table: "ActivosFijos",
                columns: new[] { "EmpresaId", "CodigoActivo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivosFijos");
        }
    }
}
