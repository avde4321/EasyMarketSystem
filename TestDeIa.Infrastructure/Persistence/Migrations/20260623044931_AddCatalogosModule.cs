using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Personas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Catalogos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogoItems_Catalogos_CatalogoId",
                        column: x => x.CatalogoId,
                        principalTable: "Catalogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Catalogos",
                columns: new[] { "Id", "Codigo", "Descripcion", "IsActive", "Nombre" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), "TIPO_IDENTIFICACION", "Tipos base de identificacion para personas y clientes.", true, "Tipo de identificacion" },
                    { new Guid("70000000-0000-0000-0000-000000000002"), "ESTADO_CIVIL", "Estado civil de personas.", true, "Estado civil" },
                    { new Guid("70000000-0000-0000-0000-000000000003"), "ESTADO_REGISTRO", "Estados funcionales de registros activos e inactivos.", true, "Estado de registro" },
                    { new Guid("70000000-0000-0000-0000-000000000004"), "ESTADO_DOCUMENTO_ELECTRONICO", "Estados de comprobantes electronicos.", true, "Estado documento electronico" },
                    { new Guid("70000000-0000-0000-0000-000000000005"), "AMBIENTE_SRI", "Ambiente de emision para comprobantes electronicos.", true, "Ambiente SRI" },
                    { new Guid("70000000-0000-0000-0000-000000000006"), "TIPO_EMISION", "Tipo de emision de documentos electronicos.", true, "Tipo de emision" },
                    { new Guid("70000000-0000-0000-0000-000000000007"), "FORMA_PAGO_SRI", "Formas de pago segun catalogo del SRI.", true, "Forma de pago SRI" }
                });

            migrationBuilder.UpdateData(
                table: "Personas",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "EstadoCivil",
                value: null);

            migrationBuilder.InsertData(
                table: "CatalogoItems",
                columns: new[] { "Id", "CatalogoId", "Codigo", "Descripcion", "IsActive", "Nombre", "Orden" },
                values: new object[,]
                {
                    { new Guid("71000000-0000-0000-0000-000000000001"), new Guid("70000000-0000-0000-0000-000000000001"), "RUC", null, true, "RUC", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000002"), new Guid("70000000-0000-0000-0000-000000000001"), "Cedula", null, true, "Cedula", 2 },
                    { new Guid("71000000-0000-0000-0000-000000000003"), new Guid("70000000-0000-0000-0000-000000000001"), "Pasaporte", null, true, "Pasaporte", 3 },
                    { new Guid("71000000-0000-0000-0000-000000000004"), new Guid("70000000-0000-0000-0000-000000000001"), "Consumidor Final", null, true, "Consumidor final", 4 },
                    { new Guid("71000000-0000-0000-0000-000000000005"), new Guid("70000000-0000-0000-0000-000000000001"), "Identificacion del Exterior", null, true, "Identificacion del exterior", 5 },
                    { new Guid("71000000-0000-0000-0000-000000000006"), new Guid("70000000-0000-0000-0000-000000000001"), "Placa", null, true, "Placa", 6 },
                    { new Guid("71000000-0000-0000-0000-000000000011"), new Guid("70000000-0000-0000-0000-000000000002"), "SOLTERO", null, true, "Soltero", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000012"), new Guid("70000000-0000-0000-0000-000000000002"), "CASADO", null, true, "Casado", 2 },
                    { new Guid("71000000-0000-0000-0000-000000000013"), new Guid("70000000-0000-0000-0000-000000000002"), "DIVORCIADO", null, true, "Divorciado", 3 },
                    { new Guid("71000000-0000-0000-0000-000000000014"), new Guid("70000000-0000-0000-0000-000000000002"), "VIUDO", null, true, "Viudo", 4 },
                    { new Guid("71000000-0000-0000-0000-000000000015"), new Guid("70000000-0000-0000-0000-000000000002"), "UNION_LIBRE", null, true, "Union libre", 5 },
                    { new Guid("71000000-0000-0000-0000-000000000021"), new Guid("70000000-0000-0000-0000-000000000003"), "ACTIVO", null, true, "Activo", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000022"), new Guid("70000000-0000-0000-0000-000000000003"), "INACTIVO", null, true, "Inactivo", 2 },
                    { new Guid("71000000-0000-0000-0000-000000000031"), new Guid("70000000-0000-0000-0000-000000000004"), "Pendiente", null, true, "Pendiente", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000032"), new Guid("70000000-0000-0000-0000-000000000004"), "EnProceso", null, true, "En proceso", 2 },
                    { new Guid("71000000-0000-0000-0000-000000000033"), new Guid("70000000-0000-0000-0000-000000000004"), "Recibido", null, true, "Recibido", 3 },
                    { new Guid("71000000-0000-0000-0000-000000000034"), new Guid("70000000-0000-0000-0000-000000000004"), "Autorizado", null, true, "Autorizado", 4 },
                    { new Guid("71000000-0000-0000-0000-000000000035"), new Guid("70000000-0000-0000-0000-000000000004"), "Rechazado", null, true, "Rechazado", 5 },
                    { new Guid("71000000-0000-0000-0000-000000000036"), new Guid("70000000-0000-0000-0000-000000000004"), "Error", null, true, "Error", 6 },
                    { new Guid("71000000-0000-0000-0000-000000000041"), new Guid("70000000-0000-0000-0000-000000000005"), "Pruebas", "Codigo SRI 1", true, "Pruebas", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000042"), new Guid("70000000-0000-0000-0000-000000000005"), "Produccion", "Codigo SRI 2", true, "Produccion", 2 },
                    { new Guid("71000000-0000-0000-0000-000000000051"), new Guid("70000000-0000-0000-0000-000000000006"), "Normal", "Codigo SRI 1", true, "Normal", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000061"), new Guid("70000000-0000-0000-0000-000000000007"), "Efectivo", "Codigo SRI 01", true, "Efectivo", 1 },
                    { new Guid("71000000-0000-0000-0000-000000000062"), new Guid("70000000-0000-0000-0000-000000000007"), "Compensacion", "Codigo SRI 15", true, "Compensacion", 2 },
                    { new Guid("71000000-0000-0000-0000-000000000063"), new Guid("70000000-0000-0000-0000-000000000007"), "Tarjeta de debito", "Codigo SRI 16", true, "Tarjeta de debito", 3 },
                    { new Guid("71000000-0000-0000-0000-000000000064"), new Guid("70000000-0000-0000-0000-000000000007"), "Dinero electronico", "Codigo SRI 17", true, "Dinero electronico", 4 },
                    { new Guid("71000000-0000-0000-0000-000000000065"), new Guid("70000000-0000-0000-0000-000000000007"), "Tarjeta prepago", "Codigo SRI 18", true, "Tarjeta prepago", 5 },
                    { new Guid("71000000-0000-0000-0000-000000000066"), new Guid("70000000-0000-0000-0000-000000000007"), "Tarjeta de credito", "Codigo SRI 19", true, "Tarjeta de credito", 6 },
                    { new Guid("71000000-0000-0000-0000-000000000067"), new Guid("70000000-0000-0000-0000-000000000007"), "Transferencia", "Codigo SRI 20", true, "Transferencia", 7 },
                    { new Guid("71000000-0000-0000-0000-000000000068"), new Guid("70000000-0000-0000-0000-000000000007"), "Endoso de titulos", "Codigo SRI 21", true, "Endoso de titulos", 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItems_CatalogoId_Codigo",
                table: "CatalogoItems",
                columns: new[] { "CatalogoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalogos_Codigo",
                table: "Catalogos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogoItems");

            migrationBuilder.DropTable(
                name: "Catalogos");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Personas");
        }
    }
}
