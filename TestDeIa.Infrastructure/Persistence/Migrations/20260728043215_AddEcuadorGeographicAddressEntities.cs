using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEcuadorGeographicAddressEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CiudadCodigo",
                table: "Personas",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinciaCodigo",
                table: "Personas",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionCodigo",
                table: "Personas",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectorCodigo",
                table: "Personas",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CiudadCodigo",
                table: "EmpresasEmisoras",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinciaCodigo",
                table: "EmpresasEmisoras",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionCodigo",
                table: "EmpresasEmisoras",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectorCodigo",
                table: "EmpresasEmisoras",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentItemId",
                table: "CatalogoItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GeoRegiones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoRegiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoProvincias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoProvincias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoProvincias_GeoRegiones_RegionId",
                        column: x => x.RegionId,
                        principalTable: "GeoRegiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeoCiudades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoCiudades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoCiudades_GeoProvincias_ProvinciaId",
                        column: x => x.ProvinciaId,
                        principalTable: "GeoProvincias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeoSectores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CiudadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoSectores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoSectores_GeoCiudades_CiudadId",
                        column: x => x.CiudadId,
                        principalTable: "GeoCiudades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000001"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000002"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000003"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000004"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000005"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000006"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000011"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000012"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000013"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000014"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000015"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000021"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000022"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000031"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000032"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000033"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000034"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000035"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000036"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000041"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000042"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000051"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000061"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000062"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000063"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000064"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000065"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000066"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000067"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000068"),
                column: "ParentItemId",
                value: null);

            migrationBuilder.UpdateData(
                table: "EmpresasEmisoras",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CiudadCodigo", "ProvinciaCodigo", "RegionCodigo", "SectorCodigo" },
                values: new object[] { "GUAYAQUIL", "GUAYAS", "COSTA", "GUAYAQUIL_NORTE" });

            migrationBuilder.UpdateData(
                table: "Personas",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CiudadCodigo", "ProvinciaCodigo", "RegionCodigo", "SectorCodigo" },
                values: new object[] { "QUITO", "PICHINCHA", "NORTE", "QUITO_NORTE" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItems_ParentItemId",
                table: "CatalogoItems",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoCiudades_Codigo",
                table: "GeoCiudades",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeoCiudades_ProvinciaId",
                table: "GeoCiudades",
                column: "ProvinciaId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoProvincias_Codigo",
                table: "GeoProvincias",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeoProvincias_RegionId",
                table: "GeoProvincias",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoRegiones_Codigo",
                table: "GeoRegiones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeoSectores_CiudadId_Codigo",
                table: "GeoSectores",
                columns: new[] { "CiudadId", "Codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogoItems_CatalogoItems_ParentItemId",
                table: "CatalogoItems",
                column: "ParentItemId",
                principalTable: "CatalogoItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogoItems_CatalogoItems_ParentItemId",
                table: "CatalogoItems");

            migrationBuilder.DropTable(
                name: "GeoSectores");

            migrationBuilder.DropTable(
                name: "GeoCiudades");

            migrationBuilder.DropTable(
                name: "GeoProvincias");

            migrationBuilder.DropTable(
                name: "GeoRegiones");

            migrationBuilder.DropIndex(
                name: "IX_CatalogoItems_ParentItemId",
                table: "CatalogoItems");

            migrationBuilder.DropColumn(
                name: "CiudadCodigo",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "ProvinciaCodigo",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "RegionCodigo",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "SectorCodigo",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "CiudadCodigo",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "ProvinciaCodigo",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "RegionCodigo",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "SectorCodigo",
                table: "EmpresasEmisoras");

            migrationBuilder.DropColumn(
                name: "ParentItemId",
                table: "CatalogoItems");
        }
    }
}
