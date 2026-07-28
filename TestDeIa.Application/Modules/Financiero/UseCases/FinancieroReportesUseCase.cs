using System.Text.Json;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Financiero.Ports.In;
using TestDeIa.Application.Modules.Financiero.Ports.Out;
using TestDeIa.Application.Modules.Financiero.Services;
using TestDeIa.Domain.Modules.Financiero.Entities;
using TestDeIa.Shared.Responses.Financiero;

namespace TestDeIa.Application.Modules.Financiero.UseCases;

public sealed class FinancieroReportesUseCase : IFinancieroReportesUseCase
{
    private readonly IFinancieroReportesRepository financieroReportesRepository;
    private readonly IAnalizadorFiscalIAService analizadorFiscalIAService;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public FinancieroReportesUseCase(
        IFinancieroReportesRepository financieroReportesRepository,
        IAnalizadorFiscalIAService analizadorFiscalIAService,
        ITenantContextAccessor tenantContextAccessor)
    {
        this.financieroReportesRepository = financieroReportesRepository;
        this.analizadorFiscalIAService = analizadorFiscalIAService;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<ConsolidadoIvaMensualResponse> ObtenerConsolidadoIvaAsync(
        int mes,
        int anio,
        string? puntoEmision = null,
        string? cajero = null,
        CancellationToken cancellationToken = default)
    {
        if (mes is < 1 or > 12)
        {
            throw new InvalidOperationException("El mes debe estar entre 1 y 12.");
        }

        if (anio < 2000 || anio > 2100)
        {
            throw new InvalidOperationException("El anio solicitado no es valido.");
        }

        var consolidadoBase = await financieroReportesRepository.ObtenerConsolidadoIvaAsync(mes, anio, puntoEmision, cajero, cancellationToken);
        var memoriasHistoricas = await financieroReportesRepository.ObtenerMemoriasHistoricasAsync(mes, anio, 3, cancellationToken);
        var analisis = analizadorFiscalIAService.Analizar(consolidadoBase, memoriasHistoricas);

        var consolidado = new ConsolidadoIvaMensual
        {
            Mes = consolidadoBase.Mes,
            Anio = consolidadoBase.Anio,
            Ventas = consolidadoBase.Ventas,
            VentasBienes = consolidadoBase.VentasBienes,
            VentasServicios = consolidadoBase.VentasServicios,
            Compras = consolidadoBase.Compras,
            ServiciosOperativos = consolidadoBase.ServiciosOperativos,
            RazonamientoIA = analisis.RazonamientoIA,
            ContextoPrevioUtilizado = analisis.ContextoPrevioUtilizado,
            TieneAprendizajeAcumulado = analisis.TieneAprendizajeAcumulado
        };

        var empresaId = tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa en el contexto actual.");
        var userId = tenantContextAccessor.UserId ?? Guid.Empty;
        var timestamp = DateTimeOffset.UtcNow;

        await financieroReportesRepository.GuardarMemoriaAnalisisFiscalAsync(
            new MemoriaAnalisisFiscal
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Mes = consolidado.Mes,
                Anio = consolidado.Anio,
                ResumenNumericoJson = JsonSerializer.Serialize(new
                {
                    VentasBaseTarifaDiferenteCero = consolidado.Ventas.BaseTarifaDiferenteCero,
                    VentasBienesBaseTarifaDiferenteCero = consolidado.VentasBienes.BaseTarifaDiferenteCero,
                    VentasServiciosBaseTarifaDiferenteCero = consolidado.VentasServicios.BaseTarifaDiferenteCero,
                    ComprasBaseTarifaDiferenteCero = consolidado.Compras.BaseTarifaDiferenteCero,
                    consolidado.DebitoFiscal,
                    consolidado.CreditoFiscal,
                    consolidado.IvaNetoPagar,
                    consolidado.CreditoTributario,
                    consolidado.ServiciosOperativos.TotalFacturadoServicios
                }),
                RazonamientoIA = consolidado.RazonamientoIA,
                ContextoPrevioUtilizado = consolidado.ContextoPrevioUtilizado,
                CreatedAt = timestamp,
                UsuarioCreacionId = userId
            },
            cancellationToken);

        return new ConsolidadoIvaMensualResponse
        {
            Mes = consolidado.Mes,
            Anio = consolidado.Anio,
            Ventas = MapTarifa(consolidado.Ventas),
            VentasBienes = MapTarifa(consolidado.VentasBienes),
            VentasServicios = MapTarifa(consolidado.VentasServicios),
            Compras = MapTarifa(consolidado.Compras),
            ServiciosOperativos = new ConsolidadoIvaServiciosOperativosResponse
            {
                PuntoEmision = consolidado.ServiciosOperativos.PuntoEmision,
                Cajero = consolidado.ServiciosOperativos.Cajero,
                TotalFacturadoServicios = consolidado.ServiciosOperativos.TotalFacturadoServicios,
                FacturasProcesadas = consolidado.ServiciosOperativos.FacturasProcesadas
            },
            RazonamientoIA = consolidado.RazonamientoIA,
            ContextoPrevioUtilizado = consolidado.ContextoPrevioUtilizado,
            TieneAprendizajeAcumulado = consolidado.TieneAprendizajeAcumulado,
            DebitoFiscal = consolidado.DebitoFiscal,
            CreditoFiscal = consolidado.CreditoFiscal,
            IvaNetoPagar = consolidado.IvaNetoPagar,
            CreditoTributario = consolidado.CreditoTributario,
            FactorProporcionalidad = consolidado.FactorProporcionalidad
        };
    }

    private static ConsolidadoIvaTarifaResponse MapTarifa(ConsolidadoIvaTarifa tarifa)
    {
        return new ConsolidadoIvaTarifaResponse
        {
            Base0 = tarifa.Base0,
            Base5 = tarifa.Base5,
            Base8 = tarifa.Base8,
            Base15 = tarifa.Base15,
            Iva5 = tarifa.Iva5,
            Iva8 = tarifa.Iva8,
            Iva15 = tarifa.Iva15,
            BaseTarifaDiferenteCero = tarifa.BaseTarifaDiferenteCero,
            IvaTarifaDiferenteCero = tarifa.IvaTarifaDiferenteCero
        };
    }
}
