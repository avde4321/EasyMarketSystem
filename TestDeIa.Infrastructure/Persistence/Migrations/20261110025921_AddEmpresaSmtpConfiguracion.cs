using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaSmtpConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresaSmtpConfiguraciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Puerto = table.Column<int>(type: "int", nullable: false),
                    UsarSsl = table.Column<bool>(type: "bit", nullable: false),
                    RequiereAutenticacion = table.Column<bool>(type: "bit", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContrasenaProtegida = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CorreoRemitente = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NombreRemitente = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Habilitado = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaSmtpConfiguraciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaSmtpConfiguraciones_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaSmtpConfiguraciones_EmpresaId",
                table: "EmpresaSmtpConfiguraciones",
                column: "EmpresaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresaSmtpConfiguraciones");
        }
    }
}
