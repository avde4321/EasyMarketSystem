using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumentoId",
                table: "ColaProcesamientoSRI",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<int>(
                name: "Intentos",
                table: "ColaProcesamientoSRI",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("""
                UPDATE ColaProcesamientoSRI
                SET Estado = CASE
                    WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('PENDIENTE') THEN 'PENDIENTE'
                    WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('EN_PROCESO', 'PROCESANDO') THEN 'EN_PROCESO'
                    WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('AUTORIZADO') THEN 'AUTORIZADO'
                    WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('ERROR') THEN 'ERROR'
                    WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('DEVUELTO', 'DEVUELTA') THEN 'DEVUELTO'
                    ELSE 'ERROR'
                END
                WHERE Estado IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "ColaProcesamientoSRI",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ColaProcesamientoSRI",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<string>(
                name: "ProcessingNode",
                table: "ColaProcesamientoSRI",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingStartedAt",
                table: "ColaProcesamientoSRI",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ColaProcesamientoSRI",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "UltimoError",
                table: "ColaProcesamientoSRI",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentosAdjuntos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Modulo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    EntidadTipo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoAdjunto = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    RutaStorage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HashSHA256 = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Origen = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosAdjuntos_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ColaProcesamientoSRI_Estado",
                table: "ColaProcesamientoSRI",
                sql: "[Estado] IN ('PENDIENTE','EN_PROCESO','AUTORIZADO','ERROR','DEVUELTO')");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAdjuntos_EmpresaId_EntidadTipo_EntidadId",
                table: "DocumentosAdjuntos",
                columns: new[] { "EmpresaId", "EntidadTipo", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAdjuntos_EmpresaId_Modulo_TipoAdjunto_FechaCreacion",
                table: "DocumentosAdjuntos",
                columns: new[] { "EmpresaId", "Modulo", "TipoAdjunto", "FechaCreacion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosAdjuntos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ColaProcesamientoSRI_Estado",
                table: "ColaProcesamientoSRI");

            migrationBuilder.DropColumn(
                name: "ProcessingNode",
                table: "ColaProcesamientoSRI");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAt",
                table: "ColaProcesamientoSRI");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ColaProcesamientoSRI");

            migrationBuilder.DropColumn(
                name: "UltimoError",
                table: "ColaProcesamientoSRI");

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumentoId",
                table: "ColaProcesamientoSRI",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<int>(
                name: "Intentos",
                table: "ColaProcesamientoSRI",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "ColaProcesamientoSRI",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ColaProcesamientoSRI",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
