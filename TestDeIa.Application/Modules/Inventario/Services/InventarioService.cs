using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Inventario;
using TestDeIa.Domain.Modules.Inventario.Entities;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Application.Modules.Inventario.Services;

public sealed class InventarioService : IInventarioService
{
    private readonly IInventarioRepository inventarioRepository;

    public InventarioService(IInventarioRepository inventarioRepository)
    {
        this.inventarioRepository = inventarioRepository;
    }

    public Task<Producto?> RegistrarEntradaCompraAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default)
    {
        return inventarioRepository.RegistrarCompraAsync(
            request.ProductoId,
            request.BodegaId,
            request.Cantidad,
            request.CostoUnitario,
            request.Referencia,
            cancellationToken);
    }

    public async Task<Producto?> RegistrarSalidaVentaAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await inventarioRepository.RegistrarMovimientoAsync(
                request.ProductoId,
                request.BodegaId,
                TipoMovimientoInventario.SalidaVenta,
                request.Concepto,
                request.Referencia,
                request.Cantidad,
                request.CostoUnitario,
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            var alternativas = await inventarioRepository.GetDisponibilidadEnOtrasBodegasAsync(
                request.ProductoId,
                request.BodegaId,
                cancellationToken);

            var detalleAlternativas = alternativas.Count == 0
                ? " No se encontro disponibilidad en otras bodegas."
                : " Disponible en otras bodegas: " + string.Join(
                    "; ",
                    alternativas.Select(item => $"{item.BodegaNombre}: {item.StockActual:0.####}"));

            throw new StockInsuficienteException($"{exception.Message}{detalleAlternativas}");
        }
    }

    public Task<Producto?> RegistrarDevolucionVentaAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default)
    {
        return inventarioRepository.RegistrarMovimientoAsync(
            request.ProductoId,
            request.BodegaId,
            TipoMovimientoInventario.DevolucionVenta,
            request.Concepto,
            request.Referencia,
            request.Cantidad,
            request.CostoUnitario,
            cancellationToken);
    }

    public Task<Producto?> RegistrarAjusteAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default)
    {
        return inventarioRepository.RegistrarMovimientoAsync(
            request.ProductoId,
            request.BodegaId,
            request.Cantidad >= 0 ? TipoMovimientoInventario.AjusteIngreso : TipoMovimientoInventario.AjusteEgreso,
            request.Concepto,
            request.Referencia,
            Math.Abs(request.Cantidad),
            request.CostoUnitario,
            cancellationToken);
    }

    public Task<Producto?> RegistrarTransferenciaSalidaAsync(InventarioTransferenciaOperacion request, CancellationToken cancellationToken = default)
    {
        return inventarioRepository.TransferirStockAsync(
            request.ProductoId,
            request.BodegaOrigenId,
            request.BodegaDestinoId,
            request.Cantidad,
            request.Referencia,
            cancellationToken);
    }

    public Task<Producto?> RegistrarTransferenciaEntradaAsync(InventarioTransferenciaOperacion request, CancellationToken cancellationToken = default)
    {
        return inventarioRepository.TransferirStockAsync(
            request.ProductoId,
            request.BodegaOrigenId,
            request.BodegaDestinoId,
            request.Cantidad,
            request.Referencia,
            cancellationToken);
    }

    public Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> ConsultarStockPorBodegaAsync(
        Guid productoId,
        Guid? bodegaId = null,
        CancellationToken cancellationToken = default)
    {
        return inventarioRepository.GetDisponibilidadEnOtrasBodegasAsync(productoId, bodegaId, cancellationToken);
    }

    public Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> ConsultarDisponibilidadOtrasBodegasAsync(
        Guid productoId,
        Guid bodegaActualId,
        CancellationToken cancellationToken = default)
    {
        return inventarioRepository.GetDisponibilidadEnOtrasBodegasAsync(productoId, bodegaActualId, cancellationToken);
    }
}
