using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppAndDigitalPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresaConfiguracionServicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WhatsAppApiToken = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    WhatsAppPhoneId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    WhatsAppBusinessAccountId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PayPhoneToken = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    PayPhoneClientAppId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PasarelaPagoActiva = table.Column<byte>(type: "tinyint", nullable: false),
                    ModopagosAmbiente = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaConfiguracionServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaConfiguracionServicios_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.CreateTable(
                name: "TransaccionesPagosDigitales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Pasarela = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionIdPasarela = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstadoPago = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LinkPagoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QrCodeBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaAprobacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesPagosDigitales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesPagosDigitales_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "PersonaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransaccionesPagosDigitales_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransaccionesPagosDigitales_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppNotificacionesLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroDestino = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoEnvio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaEnvio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppNotificacionesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppNotificacionesLog_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaConfiguracionServicios_EmpresaId",
                table: "EmpresaConfiguracionServicios",
                column: "EmpresaId",
                unique: true);


            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPagosDigitales_ClienteId",
                table: "TransaccionesPagosDigitales",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPagosDigitales_EmpresaId_EstadoPago_FechaCreacion",
                table: "TransaccionesPagosDigitales",
                columns: new[] { "EmpresaId", "EstadoPago", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPagosDigitales_EmpresaId_FacturaId",
                table: "TransaccionesPagosDigitales",
                columns: new[] { "EmpresaId", "FacturaId" });

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPagosDigitales_EmpresaId_TransactionIdPasarela",
                table: "TransaccionesPagosDigitales",
                columns: new[] { "EmpresaId", "TransactionIdPasarela" },
                unique: true,
                filter: "[TransactionIdPasarela] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPagosDigitales_FacturaId",
                table: "TransaccionesPagosDigitales",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppNotificacionesLog_EmpresaId_EstadoEnvio_FechaEnvio",
                table: "WhatsAppNotificacionesLog",
                columns: new[] { "EmpresaId", "EstadoEnvio", "FechaEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppNotificacionesLog_EmpresaId_TipoDocumento_DocumentoId",
                table: "WhatsAppNotificacionesLog",
                columns: new[] { "EmpresaId", "TipoDocumento", "DocumentoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresaConfiguracionServicios");

            migrationBuilder.DropTable(
                name: "TransaccionesPagosDigitales");

            migrationBuilder.DropTable(
                name: "WhatsAppNotificacionesLog");
        }
    }
}
