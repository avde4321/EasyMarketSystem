using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoriaAnalisisFiscalStage62Evolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemoriasAnalisisFiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    ResumenNumericoJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RazonamientoIA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContextoPrevioUtilizado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoriasAnalisisFiscal", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemoriasAnalisisFiscal_EmpresaId_Mes_Anio",
                table: "MemoriasAnalisisFiscal",
                columns: new[] { "EmpresaId", "Mes", "Anio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemoriasAnalisisFiscal");
        }
    }
}
