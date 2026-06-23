using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoreEmpresaCertificateInDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificadoRuta",
                table: "EmpresasEmisoras");

            migrationBuilder.AddColumn<byte[]>(
                name: "CertificadoContenido",
                table: "EmpresasEmisoras",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificadoNombreArchivo",
                table: "EmpresasEmisoras",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificadoContenido",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "CertificadoNombreArchivo",
                table: "EmpresasEmisoras");

            migrationBuilder.AddColumn<string>(
                name: "CertificadoRuta",
                table: "EmpresasEmisoras",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
