using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaEmisoraConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgenteRetencionResolucion",
                table: "Facturas",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmbienteSri",
                table: "Facturas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContribuyenteEspecial",
                table: "Facturas",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionEstablecimientoEmisor",
                table: "Facturas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionMatrizEmisor",
                table: "Facturas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaEmisoraId",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreComercialEmisor",
                table: "Facturas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ObligadoContabilidad",
                table: "Facturas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocialEmisor",
                table: "Facturas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegimenRimpe",
                table: "Facturas",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RucEmisor",
                table: "Facturas",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoEmision",
                table: "Facturas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmpresasEmisoras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NombreComercial = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Ruc = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    DireccionMatriz = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DireccionEstablecimiento = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PuntoEmision = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AmbienteSri = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ModoDesarrollo = table.Column<bool>(type: "bit", nullable: false),
                    TipoEmision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ObligadoContabilidad = table.Column<bool>(type: "bit", nullable: false),
                    ContribuyenteEspecial = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RegimenRimpe = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AgenteRetencionResolucion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CertificadoRuta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertificadoClave = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasEmisoras", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasEmisoras_Ruc",
                table: "EmpresasEmisoras",
                column: "Ruc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "AgenteRetencionResolucion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "AmbienteSri",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ContribuyenteEspecial",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "DireccionEstablecimientoEmisor",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "DireccionMatrizEmisor",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "EmpresaEmisoraId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "NombreComercialEmisor",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ObligadoContabilidad",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "RazonSocialEmisor",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "RegimenRimpe",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "RucEmisor",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TipoEmision",
                table: "Facturas");
        }
    }
}
