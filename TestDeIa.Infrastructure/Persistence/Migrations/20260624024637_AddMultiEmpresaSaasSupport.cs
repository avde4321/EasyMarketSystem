using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiEmpresaSaasSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_NormalizedEmail",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_NormalizedUserName",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Personas_Identificacion",
                table: "Personas");

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "SecurityUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Personas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "KardexMovimientos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "EmpresasEmisoras",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Empleados",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SecurityUserEmpresas",
                columns: table => new
                {
                    SecurityUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityUserEmpresas", x => new { x.SecurityUserId, x.EmpresaId });
                    table.ForeignKey(
                        name: "FK_SecurityUserEmpresas_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityUserEmpresas_SecurityUsers_SecurityUserId",
                        column: x => x.SecurityUserId,
                        principalTable: "SecurityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EmpresasEmisoras",
                columns: new[] { "Id", "AgenteRetencionResolucion", "AmbienteSri", "CertificadoClave", "CertificadoContenido", "CertificadoNombreArchivo", "ContribuyenteEspecial", "CreatedAt", "DireccionEstablecimiento", "DireccionMatriz", "Establecimiento", "IsActive", "ModoDesarrollo", "NombreComercial", "ObligadoContabilidad", "OwnerUserId", "PuntoEmision", "RazonSocial", "RegimenRimpe", "Ruc", "TipoEmision", "UpdatedAt" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), null, "Pruebas", null, null, null, null, new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sucursal demo", "Matriz demo", "001", true, true, "EasyMarket Demo", false, new Guid("11111111-1111-1111-1111-111111111111"), "001", "EasyMarket Demo S.A.", null, "0999999999001", "Normal", null });

            migrationBuilder.UpdateData(
                table: "Personas",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "EmpresaId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "SecurityUsers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "EmpresaId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.InsertData(
                table: "SecurityUserEmpresas",
                columns: new[] { "EmpresaId", "SecurityUserId", "CreatedAt", "IsDefault" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_EmpresaId_NormalizedEmail",
                table: "SecurityUsers",
                columns: new[] { "EmpresaId", "NormalizedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_EmpresaId_NormalizedUserName",
                table: "SecurityUsers",
                columns: new[] { "EmpresaId", "NormalizedUserName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_EmpresaId_PersonaId",
                table: "SecurityUsers",
                columns: new[] { "EmpresaId", "PersonaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId_Codigo",
                table: "Productos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_EmpresaId_Identificacion",
                table: "Personas",
                columns: new[] { "EmpresaId", "Identificacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasEmisoras_OwnerUserId_Ruc",
                table: "EmpresasEmisoras",
                columns: new[] { "OwnerUserId", "Ruc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpresaId_PersonaId",
                table: "Empleados",
                columns: new[] { "EmpresaId", "PersonaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId_PersonaId",
                table: "Clientes",
                columns: new[] { "EmpresaId", "PersonaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUserEmpresas_EmpresaId",
                table: "SecurityUserEmpresas",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityUserEmpresas");

            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_EmpresaId_NormalizedEmail",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_EmpresaId_NormalizedUserName",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_EmpresaId_PersonaId",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId_Codigo",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Personas_EmpresaId_Identificacion",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_EmpresasEmisoras_OwnerUserId_Ruc",
                table: "EmpresasEmisoras");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_EmpresaId_PersonaId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_EmpresaId_PersonaId",
                table: "Clientes");

            migrationBuilder.DeleteData(
                table: "EmpresasEmisoras",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "SecurityUsers");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "KardexMovimientos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_NormalizedEmail",
                table: "SecurityUsers",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_NormalizedUserName",
                table: "SecurityUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Identificacion",
                table: "Personas",
                column: "Identificacion",
                unique: true);
        }
    }
}
