using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProveedoresModuleStage51 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    PersonaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoRetencionIvaDefault = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodigoRetencionRentaDefault = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PermiteCredito = table.Column<bool>(type: "bit", nullable: false),
                    DiasCredito = table.Column<int>(type: "int", nullable: false),
                    EstadoProveedor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.PersonaId);
                    table.ForeignKey(
                        name: "FK_Proveedores_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_EmpresaId_PersonaId",
                table: "Proveedores",
                columns: new[] { "EmpresaId", "PersonaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proveedores");
        }
    }
}
