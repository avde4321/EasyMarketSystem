using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaPuntosEmision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresaPuntosEmision",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaEmisoraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PuntoEmision = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DireccionEstablecimiento = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaPuntosEmision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaPuntosEmision_EmpresasEmisoras_EmpresaEmisoraId",
                        column: x => x.EmpresaEmisoraId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaPuntosEmision_EmpresaEmisoraId_Establecimiento_PuntoEmision",
                table: "EmpresaPuntosEmision",
                columns: new[] { "EmpresaEmisoraId", "Establecimiento", "PuntoEmision" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO EmpresaPuntosEmision (Id, EmpresaEmisoraId, Establecimiento, PuntoEmision, DireccionEstablecimiento, IsDefault)
                SELECT NEWID(), Id, Establecimiento, PuntoEmision, DireccionEstablecimiento, CAST(1 AS bit)
                FROM EmpresasEmisoras
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM EmpresaPuntosEmision puntos
                    WHERE puntos.EmpresaEmisoraId = EmpresasEmisoras.Id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresaPuntosEmision");
        }
    }
}
