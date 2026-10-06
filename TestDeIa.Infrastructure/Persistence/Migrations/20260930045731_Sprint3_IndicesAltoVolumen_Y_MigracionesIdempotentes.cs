using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_ClaveAcceso",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_Estado_CreatedAt",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_ComprobantesRetencion_ClaveAcceso",
                table: "ComprobantesRetencion");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_ClaveAcceso",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_Estado_CreatedAt",
                table: "ComprobanteCabecera");

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "ExtractoBancarioDetalles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "ExtractoBancarioDetalles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                """
                UPDATE detalle
                SET
                    detalle.[EmpresaId] = cuenta.[EmpresaId],
                    detalle.[CuentaBancariaId] = header.[CuentaBancariaId]
                FROM [ExtractoBancarioDetalles] AS detalle
                INNER JOIN [ExtractoBancarioHeaders] AS header
                    ON detalle.[ExtractoHeaderId] = header.[Id]
                INNER JOIN [CuentasBancarias] AS cuenta
                    ON header.[CuentaBancariaId] = cuenta.[Id]
                WHERE detalle.[EmpresaId] = '00000000-0000-0000-0000-000000000000'
                   OR detalle.[CuentaBancariaId] = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_ClaveAcceso",
                table: "Facturas",
                columns: new[] { "EmpresaId", "ClaveAcceso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_ClienteId",
                table: "Facturas",
                columns: new[] { "EmpresaId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas",
                columns: new[] { "EmpresaId", "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_Estado",
                table: "Facturas",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_Estado_CreatedAt",
                table: "Facturas",
                columns: new[] { "EmpresaId", "Estado", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_FechaEmision",
                table: "Facturas",
                columns: new[] { "EmpresaId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId_Secuencial",
                table: "Facturas",
                columns: new[] { "EmpresaId", "Secuencial" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_Conciliado_FechaTransaccion",
                table: "ExtractoBancarioDetalles",
                columns: new[] { "EmpresaId", "CuentaBancariaId", "Conciliado", "FechaTransaccion" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_FechaTransaccion",
                table: "ExtractoBancarioDetalles",
                columns: new[] { "EmpresaId", "CuentaBancariaId", "FechaTransaccion" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_NumeroDocumentoRef_Monto",
                table: "ExtractoBancarioDetalles",
                columns: new[] { "EmpresaId", "CuentaBancariaId", "NumeroDocumentoRef", "Monto" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_EmpresaId_EstadoDeuda_FechaVence",
                table: "CuentasPorPagar",
                columns: new[] { "EmpresaId", "EstadoDeuda", "FechaVence" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_EmpresaId_ProveedorId_EstadoDeuda",
                table: "CuentasPorPagar",
                columns: new[] { "EmpresaId", "ProveedorId", "EstadoDeuda" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_ClaveAcceso",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "ClaveAcceso" },
                unique: true,
                filter: "[ClaveAcceso] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_EstadoSRI",
                table: "ComprobantesRetencion",
                columns: new[] { "EmpresaId", "EstadoSRI" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_ClaveAcceso",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "ClaveAcceso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_Estado",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_Estado_CreatedAt",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "Estado", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_FechaEmision",
                table: "ComprobanteCabecera",
                columns: new[] { "EmpresaId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_ClaveAccesoProveedor",
                table: "Compras",
                columns: new[] { "EmpresaId", "ClaveAccesoProveedor" },
                filter: "[ClaveAccesoProveedor] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_EstadoCompra_FechaEmision",
                table: "Compras",
                columns: new[] { "EmpresaId", "EstadoCompra", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_ProveedorId_EstadoCompra",
                table: "Compras",
                columns: new[] { "EmpresaId", "ProveedorId", "EstadoCompra" });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_ProveedorId_FechaEmision",
                table: "Compras",
                columns: new[] { "EmpresaId", "ProveedorId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_ColaProcesamientoSRI_EmpresaId_ComprobanteId_TipoDocumentoId",
                table: "ColaProcesamientoSRI",
                columns: new[] { "EmpresaId", "ComprobanteId", "TipoDocumentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ColaProcesamientoSRI_EmpresaId_Estado",
                table: "ColaProcesamientoSRI",
                columns: new[] { "EmpresaId", "Estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_ClaveAcceso",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_ClienteId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_Estado",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_Estado_CreatedAt",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_FechaEmision",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_EmpresaId_Secuencial",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_Conciliado_FechaTransaccion",
                table: "ExtractoBancarioDetalles");

            migrationBuilder.DropIndex(
                name: "IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_FechaTransaccion",
                table: "ExtractoBancarioDetalles");

            migrationBuilder.DropIndex(
                name: "IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_NumeroDocumentoRef_Monto",
                table: "ExtractoBancarioDetalles");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorPagar_EmpresaId_EstadoDeuda_FechaVence",
                table: "CuentasPorPagar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorPagar_EmpresaId_ProveedorId_EstadoDeuda",
                table: "CuentasPorPagar");

            migrationBuilder.DropIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_ClaveAcceso",
                table: "ComprobantesRetencion");

            migrationBuilder.DropIndex(
                name: "IX_ComprobantesRetencion_EmpresaId_EstadoSRI",
                table: "ComprobantesRetencion");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_ClaveAcceso",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_Estado",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_Estado_CreatedAt",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_ComprobanteCabecera_EmpresaId_FechaEmision",
                table: "ComprobanteCabecera");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_ClaveAccesoProveedor",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_EstadoCompra_FechaEmision",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_ProveedorId_EstadoCompra",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EmpresaId_ProveedorId_FechaEmision",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_ColaProcesamientoSRI_EmpresaId_ComprobanteId_TipoDocumentoId",
                table: "ColaProcesamientoSRI");

            migrationBuilder.DropIndex(
                name: "IX_ColaProcesamientoSRI_EmpresaId_Estado",
                table: "ColaProcesamientoSRI");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "ExtractoBancarioDetalles");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ExtractoBancarioDetalles");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ClaveAcceso",
                table: "Facturas",
                column: "ClaveAcceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Establecimiento_PuntoEmision_Secuencial",
                table: "Facturas",
                columns: new[] { "Establecimiento", "PuntoEmision", "Secuencial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Estado_CreatedAt",
                table: "Facturas",
                columns: new[] { "Estado", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesRetencion_ClaveAcceso",
                table: "ComprobantesRetencion",
                column: "ClaveAcceso",
                unique: true,
                filter: "[ClaveAcceso] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_ClaveAcceso",
                table: "ComprobanteCabecera",
                column: "ClaveAcceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteCabecera_Estado_CreatedAt",
                table: "ComprobanteCabecera",
                columns: new[] { "Estado", "CreatedAt" });
        }
    }
}
