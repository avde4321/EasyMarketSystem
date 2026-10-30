using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SuperAdministradorBootstrapSeguro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaasInicializaciones",
                columns: table => new
                {
                    Clave = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasInicializaciones", x => x.Clave);
                });

            migrationBuilder.InsertData(
                table: "SecurityRoles",
                columns: new[] { "Id", "IsActive", "Name", "NormalizedName" },
                values: new object[] { new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917"), true, "SuperAdministrador", "SUPERADMINISTRADOR" });

            migrationBuilder.InsertData(
                table: "SecurityRolPermisos",
                columns: new[] { "PermisoId", "RoleId" },
                values: new object[,]
                {
                    { "caja.operar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "catalogos.administrar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "clientes.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "compras.cuentas-por-pagar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "compras.estudio-mercado", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "compras.liquidaciones", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "compras.registrar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "dashboard.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "empleados.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "empresa.configurar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "facturacion.monitor", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "financiero.iva", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "inventario.ajustar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "inventario.bodegas", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "inventario.productos", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "inventario.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "personas.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "pos.facturar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "proveedores.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "reporteria.ventas", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") },
                    { "seguridad.usuarios", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM SaasInicializaciones WHERE Clave = 'SUPERADMIN')
                   OR EXISTS (SELECT 1 FROM SecurityUserRoles WHERE RoleId = '19d227e4-c5c9-4b18-9525-d0bdcfe7f917')
                    THROW 51005, 'Rollback bloqueado: el superadministrador ya fue provisionado. Preserve la cuenta y el estado de instalacion.', 1;
                """);
            migrationBuilder.DropTable(
                name: "SaasInicializaciones");

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "caja.operar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "catalogos.administrar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "clientes.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.cuentas-por-pagar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.estudio-mercado", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.liquidaciones", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.registrar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "dashboard.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "empleados.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "empresa.configurar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "facturacion.monitor", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "financiero.iva", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.ajustar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.bodegas", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.productos", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "personas.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "pos.facturar", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "proveedores.ver", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "reporteria.ventas", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "seguridad.usuarios", new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917") });

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("19d227e4-c5c9-4b18-9525-d0bdcfe7f917"));
        }
    }
}
