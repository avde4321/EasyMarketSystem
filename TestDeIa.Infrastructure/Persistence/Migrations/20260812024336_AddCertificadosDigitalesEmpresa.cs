using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificadosDigitalesEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificadosDigitalesEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Contenido = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sujeto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Emisor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroSerie = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HuellaDigital = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaInicioVigencia = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaFinVigencia = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadosDigitalesEmpresa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadosDigitalesEmpresa_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDigitalesEmpresa_EmpresaId_EsPrincipal",
                table: "CertificadosDigitalesEmpresa",
                columns: new[] { "EmpresaId", "EsPrincipal" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDigitalesEmpresa_EmpresaId_FechaFinVigencia",
                table: "CertificadosDigitalesEmpresa",
                columns: new[] { "EmpresaId", "FechaFinVigencia" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDigitalesEmpresa_IsActive_FechaFinVigencia",
                table: "CertificadosDigitalesEmpresa",
                columns: new[] { "IsActive", "FechaFinVigencia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificadosDigitalesEmpresa");
        }
    }
}
