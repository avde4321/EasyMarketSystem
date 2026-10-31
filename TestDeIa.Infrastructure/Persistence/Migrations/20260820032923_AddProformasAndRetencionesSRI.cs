using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProformasAndRetencionesSRI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComprobantesRetencion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PuntoEmision = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Secuencial = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    ClaveAcceso = table.Column<string>(type: "nvarchar(49)", maxLength: 49, nullable: true),
                    FechaEmision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AmbienteSRI = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoSRI = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(49)", maxLength: 49, nullable: true),
                    FechaAutorizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MensajeErrorSRI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRetenido = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesRetencion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesRetencion_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesRetencion_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesRetencion_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Proformas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Secuencial = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    FechaEmision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaVencimiento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    SubtotalSinImpuestos = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SubtotalIVA = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DescuentoTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proformas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proformas_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proformas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proformas_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proformas_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proformas_SecurityUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "SecurityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprobanteRetencionDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComprobanteRetencionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoImpuesto = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CodigoRetencionSRI = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BaseImponible = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PorcentajeRetencion = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ValorRetenido = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CodDocSustento = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    NumDocSustento = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FechaEmisionDocSustento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobanteRetencionDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobanteRetencionDetalles_ComprobantesRetencion_ComprobanteRetencionId",
                        column: x => x.ComprobanteRetencionId,
                        principalTable: "ComprobantesRetencion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProformaDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProformaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TarifaIVA = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ValorIVA = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProformaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProformaDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProformaDetalles_Proformas_ProformaId",
                        column: x => x.ProformaId,
                        principalTable: "Proformas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SecurityPermisos",
                columns: new[] { "Id", "Descripcion", "Modulo", "NombrePermiso" },
                values: new object[] { "reporteria.ventas", "Consulta de reportes detallados y proyecciones de ventas.", "Reporteria", "reporteria.ventas" });

            migrationBuilder.InsertData(
                table: "SecurityRolPermisos",
                columns: new[] { "PermisoId", "RoleId" },
                values: new object[,]
                {
                    { "reporteria.ventas", new Guid("22222222-2222-2222-2222-222222222222") },
                    { "reporteria.ventas", new Guid("66666666-6666-6666-6666-666666666666") },
                    { "reporteria.ventas", new Guid("77777777-7777-7777-7777-777777777777") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteRetencionDetalles_ComprobanteRetencionId_CodigoImpuesto_CodigoRetencionSRI",
                table: "ComprobanteRetencionDetalles",
                columns: new[] { "ComprobanteRetencionId", "CodigoImpuesto", "CodigoRetencionSRI" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_ClaveAcceso",
                table: "ComprobantesRetencion",
                column: "ClaveAcceso",
                unique: true,
                filter: "[ClaveAcceso] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_CompraId",
                table: "ComprobantesRetencion",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_EstadoSRI_FechaEmision",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "EstadoSRI", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_ProveedorId_FechaEmision",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "ProveedorId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_ProveedorId",
                table: "ComprobantesRetencion",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaDetalles_ProductoId",
                table: "ProformaDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaDetalles_ProformaId_ProductoId",
                table: "ProformaDetalles",
                columns: new[] { "ProformaId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_BodegaId",
                table: "Proformas",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_ClienteId",
                table: "Proformas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_EmpresaId_ClienteId_FechaEmision",
                table: "Proformas",
                columns: new[] { "EmpresaId", "ClienteId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_EmpresaId_Estado_FechaEmision",
                table: "Proformas",
                columns: new[] { "EmpresaId", "Estado", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_EmpresaId_Secuencial",
                table: "Proformas",
                columns: new[] { "EmpresaId", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_FacturaId",
                table: "Proformas",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_UsuarioId",
                table: "Proformas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobanteRetencionDetalles");

            migrationBuilder.DropTable(
                name: "ProformaDetalles");

            migrationBuilder.DropTable(
                name: "ComprobantesRetencion");

            migrationBuilder.DropTable(
                name: "Proformas");

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "reporteria.ventas", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "reporteria.ventas", new Guid("66666666-6666-6666-6666-666666666666") });

            migrationBuilder.DeleteData(
                table: "SecurityRolPermisos",
                keyColumns: new[] { "PermisoId", "RoleId" },
                keyValues: new object[] { "reporteria.ventas", new Guid("77777777-7777-7777-7777-777777777777") });

            migrationBuilder.DeleteData(
                table: "SecurityPermisos",
                keyColumn: "Id",
                keyValue: "reporteria.ventas");
        }
    }
}
