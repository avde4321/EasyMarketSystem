using System.Text.Json;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.UseCases;

public sealed class EstudioMercadoUseCase : IEstudioMercadoUseCase
{
    private readonly IEstudioMercadoRepository estudioMercadoRepository;
    private readonly IEstudioMercadoAnaliticoService estudioMercadoAnaliticoService;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public EstudioMercadoUseCase(
        IEstudioMercadoRepository estudioMercadoRepository,
        IEstudioMercadoAnaliticoService estudioMercadoAnaliticoService,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.estudioMercadoRepository = estudioMercadoRepository;
        this.estudioMercadoAnaliticoService = estudioMercadoAnaliticoService;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<EstudioMercadoCompraResponse> GenerarAsync(int mes, int anio, CancellationToken cancellationToken = default)
    {
        if (mes is < 1 or > 12)
        {
            throw new InvalidOperationException("El mes debe estar entre 1 y 12.");
        }

        if (anio is < 2000 or > 2100)
        {
            throw new InvalidOperationException("El anio solicitado no es valido.");
        }

        var now = DateTimeOffset.Now;
        var offset = now.Offset;
        var periodoInicio = new DateTimeOffset(anio, mes, 1, 0, 0, 0, offset);
        var periodoFin = periodoInicio.AddMonths(1);
        var periodoAnalisisInicio = periodoInicio.AddMonths(-2);
        var proximoMes = periodoInicio.AddMonths(1);
        var diasMesProximo = DateTime.DaysInMonth(proximoMes.Year, proximoMes.Month);

        var topProductos = await estudioMercadoRepository.GetTopProductosVendidosAsync(periodoInicio, periodoFin, 5, cancellationToken);
        var productoIds = topProductos.Select(current => current.ProductoId).ToArray();
        var historial = await estudioMercadoRepository.GetHistorialConsumoAsync(periodoAnalisisInicio, periodoFin, productoIds, cancellationToken);
        var estudiosHistoricos = await estudioMercadoRepository.GetEstudiosHistoricosAsync(mes, anio, 3, cancellationToken);
        var analisis = await estudioMercadoAnaliticoService.GenerarAsync(topProductos, historial, estudiosHistoricos, diasMesProximo, cancellationToken);

        var timestamp = DateTimeOffset.UtcNow;
        var userId = currentUserAccessor.GetRequiredUserId();
        var estudio = new EstudioMercadoCompra(
            Guid.NewGuid(),
            currentUserAccessor.GetRequiredEmpresaId(),
            anio,
            mes,
            JsonSerializer.Serialize(topProductos),
            JsonSerializer.Serialize(analisis.SugerenciasCompra),
            analisis.AnalisisEstrategicoIA,
            timestamp,
            userId,
            timestamp,
            userId,
            topProductos,
            analisis.SugerenciasCompra);

        await estudioMercadoRepository.SaveAsync(estudio, cancellationToken);

        return new EstudioMercadoCompraResponse
        {
            Id = estudio.Id,
            Anio = estudio.Anio,
            Mes = estudio.Mes,
            TopProductosVendidos = estudio.TopProductosVendidos.Select(current => new EstudioMercadoTopProductoResponse
            {
                ProductoId = current.ProductoId,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                CantidadVendida = current.CantidadVendida,
                TotalVendido = current.TotalVendido,
                CostoEstimado = current.CostoEstimado,
                MargenEstimado = current.MargenEstimado,
                StockActual = current.StockActual
            }).ToArray(),
            SugerenciasCompra = estudio.SugerenciasCompra.Select(current => new EstudioMercadoSugerenciaCompraResponse
            {
                ProductoId = current.ProductoId,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                CantidadRecomendada = current.CantidadRecomendada,
                CostoEstimado = current.CostoEstimado,
                JustificacionAlgoritmo = current.JustificacionAlgoritmo
            }).ToArray(),
            AnalisisEstrategicoIA = estudio.AnalisisEstrategicoIA
        };
    }
}
