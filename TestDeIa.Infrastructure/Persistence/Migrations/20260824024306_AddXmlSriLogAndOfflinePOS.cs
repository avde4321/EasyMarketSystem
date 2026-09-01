using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddXmlSriLogAndOfflinePOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacturaCompraXmlLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaveAcceso = table.Column<string>(type: "nvarchar(49)", maxLength: 49, nullable: false),
                    RucEmisor = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RazonSocialEmisor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RucComprador = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    FechaEmision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CodDoc = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    EstabPuntoEmiSecuencial = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    TotalSinImpuestos = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalDescuento = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ImporteTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    XmlContenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoProcesamiento = table.Column<byte>(type: "tinyint", nullable: false),
                    CompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaCompraXmlLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaCompraXmlLogs_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaCompraXmlLogs_CompraId",
                table: "FacturaCompraXmlLogs",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso",
                table: "FacturaCompraXmlLogs",
                columns: new[] { "EmpresaId", "ClaveAcceso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision",
                table: "FacturaCompraXmlLogs",
                columns: new[] { "EmpresaId", "EstadoProcesamiento", "FechaEmision" });

            migrationBuilder.InsertData(
                table: "Catalogos",
                columns: new[] { "Id", "Codigo", "Descripcion", "IsActive", "Nombre" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000008"), "RETENCION_IVA_SRI", "Codigos base de retencion de IVA para documentos de proveedor.", true, "Retenciones IVA SRI" },
                    { new Guid("70000000-0000-0000-0000-000000000009"), "RETENCION_RENTA_SRI", "Codigos base de retencion en la fuente para documentos de proveedor.", true, "Retenciones Renta SRI" }
                });

            migrationBuilder.InsertData(
                table: "CatalogoItems",
                columns: new[] { "Id", "CatalogoId", "Codigo", "Descripcion", "IsActive", "Nombre", "Orden", "ParentItemId" },
                values: new object[,]
                {
                    { new Guid("71000000-0000-0000-0000-000000000081"), new Guid("70000000-0000-0000-0000-000000000008"), "0", "0%", true, "Sin retencion IVA", 1, null },
                    { new Guid("71000000-0000-0000-0000-000000000082"), new Guid("70000000-0000-0000-0000-000000000008"), "10", "10%", true, "Retencion IVA 10%", 2, null },
                    { new Guid("71000000-0000-0000-0000-000000000083"), new Guid("70000000-0000-0000-0000-000000000008"), "20", "20%", true, "Retencion IVA 20%", 3, null },
                    { new Guid("71000000-0000-0000-0000-000000000084"), new Guid("70000000-0000-0000-0000-000000000008"), "30", "30%", true, "Retencion IVA 30%", 4, null },
                    { new Guid("71000000-0000-0000-0000-000000000085"), new Guid("70000000-0000-0000-0000-000000000008"), "50", "50%", true, "Retencion IVA 50%", 5, null },
                    { new Guid("71000000-0000-0000-0000-000000000086"), new Guid("70000000-0000-0000-0000-000000000008"), "70", "70%", true, "Retencion IVA 70%", 6, null },
                    { new Guid("71000000-0000-0000-0000-000000000087"), new Guid("70000000-0000-0000-0000-000000000008"), "100", "100%", true, "Retencion IVA 100%", 7, null },
                    { new Guid("71000000-0000-0000-0000-000000000091"), new Guid("70000000-0000-0000-0000-000000000009"), "0", "0%", true, "Sin retencion renta", 1, null },
                    { new Guid("71000000-0000-0000-0000-000000000092"), new Guid("70000000-0000-0000-0000-000000000009"), "312", "1.75%", true, "Retencion renta codigo 312", 2, null },
                    { new Guid("71000000-0000-0000-0000-000000000093"), new Guid("70000000-0000-0000-0000-000000000009"), "320", "1.75%", true, "Retencion renta codigo 320", 3, null },
                    { new Guid("71000000-0000-0000-0000-000000000094"), new Guid("70000000-0000-0000-0000-000000000009"), "322", "1.75%", true, "Retencion renta codigo 322", 4, null },
                    { new Guid("71000000-0000-0000-0000-000000000095"), new Guid("70000000-0000-0000-0000-000000000009"), "332", "1.75%", true, "Bienes codigo 332", 5, null },
                    { new Guid("71000000-0000-0000-0000-000000000096"), new Guid("70000000-0000-0000-0000-000000000009"), "343", "2.75%", true, "Servicios codigo 343", 6, null },
                    { new Guid("71000000-0000-0000-0000-000000000097"), new Guid("70000000-0000-0000-0000-000000000009"), "344", "2.75%", true, "Servicios codigo 344", 7, null },
                    { new Guid("71000000-0000-0000-0000-000000000098"), new Guid("70000000-0000-0000-0000-000000000009"), "3440", "70%", true, "Retencion IVA codigo 3440", 8, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000081"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000082"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000083"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000084"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000085"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000086"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000087"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000091"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000092"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000093"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000094"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000095"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000096"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000097"));

            migrationBuilder.DeleteData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000098"));

            migrationBuilder.DeleteData(
                table: "Catalogos",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Catalogos",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000009"));

            migrationBuilder.DropTable(
                name: "FacturaCompraXmlLogs");
        }
    }
}
