using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint5_CatalogoEstadosSri_Y_StateEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ColaProcesamientoSRI_Estado",
                table: "ColaProcesamientoSRI");

            migrationBuilder.CreateTable(
                name: "SriCatalogoErrores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodigoSri = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    MensajeSri = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SolucionSugerida = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoError = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SriCatalogoErrores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SriEstadosComprobante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RequiereReenvioRecepcion = table.Column<bool>(type: "bit", nullable: false),
                    RequiereConsultaAutorizacion = table.Column<bool>(type: "bit", nullable: false),
                    EsEstadoFinal = table.Column<bool>(type: "bit", nullable: false),
                    EsEditable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SriEstadosComprobante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SriHistorialEstadosComprobante",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComprobanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumentoId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    EstadoAnteriorId = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevoId = table.Column<int>(type: "int", nullable: false),
                    CodigoErrorSri = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    MensajeRespuesta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaTransaccion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkerNode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SriHistorialEstadosComprobante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SriHistorialEstadosComprobante_SriEstadosComprobante_EstadoAnteriorId",
                        column: x => x.EstadoAnteriorId,
                        principalTable: "SriEstadosComprobante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SriHistorialEstadosComprobante_SriEstadosComprobante_EstadoNuevoId",
                        column: x => x.EstadoNuevoId,
                        principalTable: "SriEstadosComprobante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SriCatalogoErrores",
                columns: new[] { "Id", "CodigoSri", "MensajeSri", "SolucionSugerida", "TipoError" },
                values: new object[,]
                {
                    { 1, "35", "CLAVE ACCESO REGISTRADA", "Consultar autorización con la misma clave de acceso antes de reemitir. Si ya está autorizado, recuperar XML autorizado.", "Advertencia" },
                    { 2, "43", "CLAVE DE ACCESO INVALIDA", "Validar fecha, RUC, ambiente, establecimiento, punto de emisión, secuencial, tipo de emisión y dígito verificador módulo 11.", "ErrorRecepcion" },
                    { 3, "45", "SECUENCIAL REGISTRADO", "Verificar que el secuencial no haya sido usado previamente para el mismo establecimiento y punto de emisión.", "ErrorRecepcion" },
                    { 4, "70", "FIRMA INVALIDA", "Revisar vigencia del certificado, contraseña, algoritmo de firma XAdES-BES y que el XML no se haya alterado después de firmar.", "ErrorFirma" }
                });

            migrationBuilder.InsertData(
                table: "SriEstadosComprobante",
                columns: new[] { "Id", "Codigo", "Descripcion", "EsEditable", "EsEstadoFinal", "Nombre", "RequiereConsultaAutorizacion", "RequiereReenvioRecepcion" },
                values: new object[,]
                {
                    { 1, "GENERADO", "XML generado localmente y listo para firma.", true, false, "Generado", false, false },
                    { 2, "FIRMADO", "XML firmado electrónicamente y pendiente de recepción SRI.", false, false, "Firmado", false, true },
                    { 3, "DEVUELTA", "Recepción SRI devolvió el comprobante por errores de estructura, clave o firma.", true, false, "Devuelta", false, true },
                    { 4, "EN_PROCESO", "Comprobante recibido por el SRI y pendiente de autorización.", false, false, "En proceso", true, false },
                    { 5, "AUTORIZADO", "Comprobante autorizado por el SRI.", false, true, "Autorizado", false, false },
                    { 6, "NO_AUTORIZADO", "Autorización SRI rechazó el comprobante.", true, false, "No autorizado", true, false },
                    { 7, "ANULADO", "Comprobante anulado o marcado como sin validez operativa.", false, true, "Anulado", false, false }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ColaProcesamientoSRI_Estado",
                table: "ColaProcesamientoSRI",
                sql: "[Estado] IN ('PENDIENTE','GENERADO','FIRMADO','EN_PROCESO','AUTORIZADO','NO_AUTORIZADO','ANULADO','ERROR','DEVUELTO','DEVUELTA')");

            migrationBuilder.CreateIndex(
                name: "IX_SriCatalogoErrores_CodigoSri",
                table: "SriCatalogoErrores",
                column: "CodigoSri",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SriEstadosComprobante_Codigo",
                table: "SriEstadosComprobante",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SriHistorialEstadosComprobante_EmpresaId_ComprobanteId_FechaTransaccion",
                table: "SriHistorialEstadosComprobante",
                columns: new[] { "EmpresaId", "ComprobanteId", "FechaTransaccion" });

            migrationBuilder.CreateIndex(
                name: "IX_SriHistorialEstadosComprobante_EmpresaId_EstadoNuevoId_FechaTransaccion",
                table: "SriHistorialEstadosComprobante",
                columns: new[] { "EmpresaId", "EstadoNuevoId", "FechaTransaccion" });

            migrationBuilder.CreateIndex(
                name: "IX_SriHistorialEstadosComprobante_EstadoAnteriorId",
                table: "SriHistorialEstadosComprobante",
                column: "EstadoAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_SriHistorialEstadosComprobante_EstadoNuevoId",
                table: "SriHistorialEstadosComprobante",
                column: "EstadoNuevoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SriCatalogoErrores");

            migrationBuilder.DropTable(
                name: "SriHistorialEstadosComprobante");

            migrationBuilder.DropTable(
                name: "SriEstadosComprobante");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ColaProcesamientoSRI_Estado",
                table: "ColaProcesamientoSRI");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ColaProcesamientoSRI_Estado",
                table: "ColaProcesamientoSRI",
                sql: "[Estado] IN ('PENDIENTE','EN_PROCESO','AUTORIZADO','ERROR','DEVUELTO')");
        }
    }
}
