using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonaNaturalEmpresaClienteSeparation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresasCliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NombreComercial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Ruc = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RepresentanteLegal = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    ObligadoLlevarContabilidad = table.Column<bool>(type: "bit", nullable: false),
                    ContribuyenteEspecial = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EmailFacturacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DireccionMatriz = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasCliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonasNaturales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimerNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SegundoNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PrimerApellido = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SegundoApellido = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TipoDocumento = table.Column<int>(type: "int", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    TieneRuc = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonasNaturales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasCliente_EmpresaId_Ruc",
                table: "EmpresasCliente",
                columns: new[] { "EmpresaId", "Ruc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonasNaturales_EmpresaId_NumeroDocumento",
                table: "PersonasNaturales",
                columns: new[] { "EmpresaId", "NumeroDocumento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresasCliente");

            migrationBuilder.DropTable(
                name: "PersonasNaturales");
        }
    }
}
