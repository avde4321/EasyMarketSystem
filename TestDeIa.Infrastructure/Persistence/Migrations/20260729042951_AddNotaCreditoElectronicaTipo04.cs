using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotaCreditoElectronicaTipo04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColaProcesamientoSRI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComprobanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumentoId = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Mensaje = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColaProcesamientoSRI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComprobanteCabecera",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumentoId = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Secuencial = table.Column<long>(type: "bigint", nullable: false),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PuntoEmision = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ComprobanteModificadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoModificacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CodDocModificado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    NumDocModificado = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    FechaEmisionDocSustento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RucEmisor = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RazonSocialEmisor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NombreComercialEmisor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionMatrizEmisor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DireccionEstablecimientoEmisor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AmbienteSri = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoEmision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ObligadoContabilidad = table.Column<bool>(type: "bit", nullable: false),
                    ClienteTipoIdentificacion = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ClienteIdentificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ClienteDireccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDescuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IvaTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClaveAcceso = table.Column<string>(type: "nvarchar(49)", maxLength: 49, nullable: false),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    XmlGenerado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlFirmado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaEmision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaAutorizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobanteCabecera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobanteCabecera_Facturas_ComprobanteModificadoId",
                        column: x => x.ComprobanteModificadoId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprobanteDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComprobanteCabeceraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaDetalleOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoProducto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CodigoIva = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PorcentajeIva = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IvaValor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobanteDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobanteDetalle_ComprobanteCabecera_ComprobanteCabeceraId",
                        column: x => x.ComprobanteCabeceraId,
                        principalTable: "ComprobanteCabecera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SecurityRoles",
                columns: new[] { "Id", "IsActive", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), true, "Gerente", "GERENTE" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), true, "AsesorComercial", "ASESORCOMERCIAL" }
                });

            migrationBuilder.InsertData(
                table: "SecurityRolPermisos",
                columns: new[] { "PermisoId", "RoleId" },
                values: new object[,]
                {
                    { "caja.operar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "catalogos.administrar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "clientes.ver", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "compras.cuentas-por-pagar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "compras.estudio-mercado", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "compras.liquidaciones", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "compras.registrar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "dashboard.ver", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "empleados.ver", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "empresa.configurar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "facturacion.monitor", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "financiero.iva", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "inventario.ajustar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "inventario.bodegas", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "inventario.productos", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "inventario.ver", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "personas.ver", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "pos.facturar", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "proveedores.ver", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "seguridad.usuarios", new Guid("77777777-7777-7777-7777-777777777777") },
                    { "clientes.ver", new Guid("88888888-8888-8888-8888-888888888888") },
                    { "dashboard.ver", new Guid("88888888-8888-8888-8888-888888888888") },
                    { "facturacion.monitor", new Guid("88888888-8888-8888-8888-888888888888") },
                    { "pos.facturar", new Guid("88888888-8888-8888-8888-888888888888") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColaProcesamientoSRI_ComprobanteId_TipoDocumentoId",
                table: "ColaProcesamientoSRI",
                columns: new[] { "ComprobanteId", "TipoDocumentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ColaProcesamientoSRI_EmpresaId_Estado_NextRetryAt_CreatedAt",
                table: "ColaProcesamientoSRI",
                columns: new[] { "EmpresaId", "Estado", "NextRetryAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_ClaveAcceso",
                table: "ComprobanteCabecera",
                column: "ClaveAcceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_ComprobanteModificadoId",
                table: "ComprobanteCabecera",
                column: "ComprobanteModificadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_TipoDocumentoId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "TipoDocumentoId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_Estado_CreatedAt",
                table: "ComprobanteCabecera",
                columns: new[] { "Estado", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteDetalle_ComprobanteCabeceraId",
                table: "ComprobanteDetalle",
                column: "ComprobanteCabeceraId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteDetalle_FacturaDetalleOrigenId",
                table: "ComprobanteDetalle",
                column: "FacturaDetalleOrigenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColaProcesamientoSRI");

            migrationBuilder.DropTable(
                name: "ComprobanteDetalle");

            migrationBuilder.DropTable(
                name: "ComprobanteCabecera");

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "caja.operar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "catalogos.administrar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "clientes.ver", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.cuentas-por-pagar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.estudio-mercado", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.liquidaciones", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "compras.registrar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "dashboard.ver", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "empleados.ver", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "empresa.configurar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "facturacion.monitor", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "financiero.iva", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.ajustar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.bodegas", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.productos", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "inventario.ver", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "personas.ver", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "pos.facturar", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "proveedores.ver", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "seguridad.usuarios", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "clientes.ver", new Guid("88888888-8888-8888-8888-888888888888") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "dashboard.ver", new Guid("88888888-8888-8888-8888-888888888888") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "facturacion.monitor", new Guid("88888888-8888-8888-8888-888888888888") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "pos.facturar", new Guid("88888888-8888-8888-8888-888888888888") });

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "SecurityRoles",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));
        }
    }
}
