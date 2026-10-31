using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SaasBillingPagosYActivacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaReinicioCupo",
                table: "SaasSuscripciones",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SaasPagoRegistros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaasPlanId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    EstadoPago = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    ComprobanteReferencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RegistradoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionesConfirmacion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasPagoRegistros", x => x.Id);
                    table.CheckConstraint("CK_SaasPagoRegistros_Estado", "[EstadoPago] IN ('Pendiente','Aprobado','Rechazado')");
                    table.CheckConstraint("CK_SaasPagoRegistros_Metodo", "[MetodoPago] IN ('TarjetaPasarela','TransferenciaBancaria')");
                    table.CheckConstraint("CK_SaasPagoRegistros_Monto", "[Monto] > 0");
                    table.ForeignKey(
                        name: "FK_SaasPagoRegistros_EmpresasEmisoras_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasEmisoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaasPagoRegistros_SaasPlanes_SaasPlanId",
                        column: x => x.SaasPlanId,
                        principalTable: "SaasPlanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "SaasSuscripciones",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "FechaReinicioCupo",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_SaasPagoRegistros_EmpresaId_EstadoPago_FechaPago",
                table: "SaasPagoRegistros",
                columns: new[] { "EmpresaId", "EstadoPago", "FechaPago" });

            migrationBuilder.CreateIndex(
                name: "IX_SaasPagoRegistros_EmpresaId_MetodoPago_ComprobanteReferencia",
                table: "SaasPagoRegistros",
                columns: new[] { "EmpresaId", "MetodoPago", "ComprobanteReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaasPagoRegistros_SaasPlanId",
                table: "SaasPagoRegistros",
                column: "SaasPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM SaasPagoRegistros)
                    THROW 51002, 'Rollback bloqueado: existen pagos SaaS. Preserve el historial financiero antes de cambiar el esquema.', 1;
                """);
            migrationBuilder.DropTable(
                name: "SaasPagoRegistros");

            migrationBuilder.DropColumn(
                name: "FechaReinicioCupo",
                table: "SaasSuscripciones");
        }
    }
}
