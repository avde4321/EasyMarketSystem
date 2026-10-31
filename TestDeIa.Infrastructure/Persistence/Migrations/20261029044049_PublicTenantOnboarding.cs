using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PublicTenantOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT NormalizedEmail FROM SecurityUsers GROUP BY NormalizedEmail HAVING COUNT(*) > 1)
                   OR EXISTS (SELECT NormalizedUserName FROM SecurityUsers GROUP BY NormalizedUserName HAVING COUNT(*) > 1)
                    THROW 51000, 'Existen correos o usuarios duplicados entre empresas. El DBA debe resolverlos antes de habilitar el registro publico. No se eliminaron registros.', 1;
                """);
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "SecurityUsers",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "SecurityUsers",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AddColumn<string>(
                name: "NombreCaja",
                table: "EmpresaPuntosEmision",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Caja General");

            migrationBuilder.InsertData(
                table: "SecurityRoles",
                columns: new[] { "Id", "IsActive", "Name", "NormalizedName" },
                values: new object[] { new Guid("99999999-9999-4999-8999-999999999999"), true, "AdministradorEmpresa", "ADMINISTRADOREMPRESA" });

            migrationBuilder.InsertData(
                table: "SecurityRolPermisos",
                columns: new[] { "PermisoId", "RoleId" },
                values: new object[,]
                {
                    { "caja.operar", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "clientes.ver", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "compras.cuentas-por-pagar", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "compras.estudio-mercado", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "compras.liquidaciones", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "compras.registrar", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "dashboard.ver", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "empleados.ver", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "empresa.configurar", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "facturacion.monitor", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "financiero.iva", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "inventario.ajustar", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "inventario.bodegas", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "inventario.productos", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "inventario.ver", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "personas.ver", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "pos.facturar", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "proveedores.ver", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "reporteria.ventas", new Guid("99999999-9999-4999-8999-999999999999") },
                    { "seguridad.usuarios", new Guid("99999999-9999-4999-8999-999999999999") }
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM SecurityUsers WHERE LEN(UserName) > 80 OR LEN(NormalizedUserName) > 80)
                   OR EXISTS (SELECT 1 FROM SecurityUserRoles WHERE RoleId = '99999999-9999-4999-8999-999999999999')
                    THROW 51001, 'Rollback bloqueado: hay usuarios creados con auto-registro. Se requiere un plan de compatibilidad sin perdida de datos.', 1;
                """);
            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_NormalizedEmail",
                table: "SecurityUsers");

            migrationBuilder.DropIndex(
                name: "IX_SecurityUsers_NormalizedUserName",
                table: "SecurityUsers");

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "caja.operar", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "clientes.ver", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.cuentas-por-pagar", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.estudio-mercado", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.liquidaciones", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.registrar", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "dashboard.ver", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "empleados.ver", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "empresa.configurar", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "facturacion.monitor", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "financiero.iva", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.ajustar", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.bodegas", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.productos", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.ver", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "personas.ver", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "pos.facturar", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "proveedores.ver", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "reporteria.ventas", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "seguridad.usuarios", new Guid("99999999-9999-4999-8999-999999999999") });

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-4999-8999-999999999999"));

            migrationBuilder.DropColumn(
                name: "NombreCaja",
                table: "EmpresaPuntosEmision");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "SecurityUsers",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "SecurityUsers",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180);
        }
    }
}
