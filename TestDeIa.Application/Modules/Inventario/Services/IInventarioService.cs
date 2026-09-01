using TestDeIa.Domain.Modules.Inventario.Entities;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Application.Modules.Inventario.Services;

public interface IInventarioService
{
    Task<Producto?> RegistrarEntradaCompraAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarSalidaVentaAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarDevolucionVentaAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarAjusteAsync(InventarioMovimientoOperacion request, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarTransferenciaSalidaAsync(InventarioTransferenciaOperacion request, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarTransferenciaEntradaAsync(InventarioTransferenciaOperacion request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> ConsultarStockPorBodegaAsync(
        Guid productoId,
        Guid? bodegaId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> ConsultarDisponibilidadOtrasBodegasAsync(
        Guid productoId,
        Guid bodegaActualId,
        CancellationToken cancellationToken = default);
}

public sealed record InventarioMovimientoOperacion(
    Guid ProductoId,
    Guid BodegaId,
    decimal Cantidad,
    decimal CostoUnitario,
    string Concepto,
    string? Referencia,
    Guid? DocumentoId = null);

public sealed record InventarioTransferenciaOperacion(
    Guid ProductoId,
    Guid BodegaOrigenId,
    Guid BodegaDestinoId,
    decimal Cantidad,
    string? Referencia,
    Guid? TransferenciaInventarioId = null);
