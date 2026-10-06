using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint9_SaasPlanes_Licencias_Cuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaasPlanes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LimiteFacturasMensuales = table.Column<int>(type: "int", nullable: false),
                    LimiteUsuarios = table.Column<int>(type: "int", nullable: false),
                    LimiteSucursales = table.Column<int>(type: "int", nullable: false),
                    PermiteModuloSRI = table.Column<bool>(type: "bit", nullable: false),
                    PermiteModuloKardexAvanzado = table.Column<bool>(type: "bit", nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasPlanes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaasSuscripciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaasPlanId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoSuscripcion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FacturasEmitidasMesActual = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ContadorFacturasPeriodo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasSuscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaasSuscripciones_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaasSuscripciones_SaasPlanes_SaasPlanId",
                        column: x => x.SaasPlanId,
                        principalTable: "SaasPlanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SaasPlanes",
                columns: new[] { "Id", "EsActivo", "LimiteFacturasMensuales", "LimiteSucursales", "LimiteUsuarios", "Nombre", "PermiteModuloKardexAvanzado", "PermiteModuloSRI", "PrecioMensual" },
                values: new object[,]
                {
                    { 1, true, 100, 1, 3, "Básico", false, true, 29.99m },
                    { 2, true, 1000, 5, 15, "Pro", true, true, 79.99m },
                    { 3, true, 0, 0, 0, "Enterprise", true, true, 0m }
                });

            migrationBuilder.InsertData(
                table: "SaasSuscripciones",
                columns: new[] { "Id", "ContadorFacturasPeriodo", "EmpresaId", "EstadoSuscripcion", "FechaInicio", "FechaVencimiento", "SaasPlanId" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Activa", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2099, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), 3 });

            migrationBuilder.CreateIndex(
                name: "IX_SaasPlanes_Nombre",
                table: "SaasPlanes",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaasSuscripciones_EmpresaId_EstadoSuscripcion",
                table: "SaasSuscripciones",
                columns: new[] { "EmpresaId", "EstadoSuscripcion" });

            migrationBuilder.CreateIndex(
                name: "IX_SaasSuscripciones_SaasPlanId",
                table: "SaasSuscripciones",
                column: "SaasPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaasSuscripciones");

            migrationBuilder.DropTable(
                name: "SaasPlanes");
        }
    }
}
