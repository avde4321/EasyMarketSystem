using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityPermissionsStage72 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityPermisos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NombrePermiso = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityPermisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityRolPermisos",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermisoId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityRolPermisos", x => new { x.RoleId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_SecurityRolPermisos_SecurityPermisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "SecurityPermisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityRolPermisos_SecurityRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SecurityRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SecurityPermisos",
                columns: new[] { "Id", "Descripcion", "Modulo", "NombrePermiso" },
                values: new object[,]
                {
                    { "caja.operar", "Apertura, cierre y control de caja.", "Caja", "caja.operar" },
                    { "catalogos.administrar", "Administracion de catalogos internos.", "Configuracion", "catalogos.administrar" },
                    { "clientes.ver", "Consulta y mantenimiento de clientes.", "Comercial", "clientes.ver" },
                    { "compras.cuentas-por-pagar", "Control de cuentas por pagar y abonos.", "Compras", "compras.cuentas-por-pagar" },
                    { "compras.estudio-mercado", "Analitica IA y estudio de mercado.", "Compras", "compras.estudio-mercado" },
                    { "compras.liquidaciones", "Emision y consulta de liquidaciones de compra.", "Compras", "compras.liquidaciones" },
                    { "compras.registrar", "Registro de compras y documentos de proveedor.", "Compras", "compras.registrar" },
                    { "dashboard.ver", "Acceso al dashboard principal.", "Dashboard", "dashboard.ver" },
                    { "empleados.ver", "Consulta y mantenimiento de empleados.", "Comercial", "empleados.ver" },
                    { "empresa.configurar", "Configuracion de empresa emisora y puntos de emision.", "Configuracion", "empresa.configurar" },
                    { "facturacion.monitor", "Consulta del monitor de comprobantes.", "Ventas", "facturacion.monitor" },
                    { "financiero.iva", "Consulta del reporte mensual de IVA.", "Financiero", "financiero.iva" },
                    { "inventario.ajustar", "Ajustes, mermas, transferencias y tomas fisicas.", "Inventario", "inventario.ajustar" },
                    { "inventario.bodegas", "Administracion de bodegas.", "Inventario", "inventario.bodegas" },
                    { "inventario.productos", "Creacion y actualizacion de productos.", "Inventario", "inventario.productos" },
                    { "inventario.ver", "Consulta de inventario y kardex.", "Inventario", "inventario.ver" },
                    { "personas.ver", "Consulta y mantenimiento de personas.", "Comercial", "personas.ver" },
                    { "pos.facturar", "Operacion del punto de venta y facturacion.", "Ventas", "pos.facturar" },
                    { "proveedores.ver", "Consulta y mantenimiento de proveedores.", "Comercial", "proveedores.ver" },
                    { "seguridad.usuarios", "Administracion de usuarios, roles y reseteo de claves.", "Seguridad", "seguridad.usuarios" }
                });

            migrationBuilder.InsertData(
                table: "SecurityRoles",
                columns: new[] { "Id", "IsActive", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), true, "Cajero", "CAJERO" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), true, "Bodeguero", "BODEGUERO" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), true, "Contador", "CONTADOR" }
                });

            migrationBuilder.InsertData(
                table: "SecurityRolPermisos",
                columns: new[] { "PermisoId", "RoleId" },
                values: new object[,]
                {
                    { "caja.operar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "catalogos.administrar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "clientes.ver", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "compras.cuentas-por-pagar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "compras.estudio-mercado", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "compras.liquidaciones", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "compras.registrar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "dashboard.ver", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "empleados.ver", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "empresa.configurar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "facturacion.monitor", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "financiero.iva", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "inventario.ajustar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "inventario.bodegas", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "inventario.productos", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "inventario.ver", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "personas.ver", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "pos.facturar", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "proveedores.ver", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "seguridad.usuarios", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "caja.operar", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "clientes.ver", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "dashboard.ver", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "facturacion.monitor", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "pos.facturar", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "dashboard.ver", new Guid("55555555-5555-5555-5555-555555555555") },
                    { "inventario.ajustar", new Guid("55555555-5555-5555-5555-555555555555") },
                    { "inventario.bodegas", new Guid("55555555-5555-5555-5555-555555555555") },
                    { "inventario.productos", new Guid("55555555-5555-5555-5555-555555555555") },
                    { "inventario.ver", new Guid("55555555-5555-5555-5555-555555555555") },
                    { "compras.cuentas-por-pagar", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "compras.liquidaciones", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "compras.registrar", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "dashboard.ver", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "empresa.configurar", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "facturacion.monitor", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "financiero.iva", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "proveedores.ver", new Guid("66666666-6666-6666-6666-666666666666") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPermisos_NombrePermiso",
                table: "SecurityPermisos",
                column: "NombrePermiso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityRolPermisos_PermisoId",
                table: "SecurityRolPermisos",
                column: "PermisoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityRolPermisos");

            migrationBuilder.DropTable(
                name: "SecurityPermisos");

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));
        }
    }
}
