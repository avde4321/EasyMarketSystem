using TestDeIa.Shared.Responses.Common;
using TestDeIa.Domain.Modules.Inventario.Entities;

namespace TestDeIa.Application.Modules.Inventario.Ports.Out;

public interface IInventarioRepository
{
    Task<IReadOnlyCollection<Producto>> GetProductosAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<Producto>> GetProductosPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<Producto?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsProductoCodigoAsync(string codigo, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<Producto> CreateProductoAsync(Producto producto, decimal stockInicial, decimal costoInicial, CancellationToken cancellationToken = default);

    Task<Producto?> UpdateProductoAsync(Producto producto, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<KardexMovimiento>> GetKardexAsync(Guid productoId, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarMovimientoAsync(
        Guid productoId,
        string tipoMovimiento,
        string concepto,
        string? referencia,
        decimal cantidad,
        decimal costoUnitario,
        CancellationToken cancellationToken = default);

    Task DescontarStockPorFacturaAsync(
        Guid facturaId,
        string referenciaFactura,
        string concepto,
        IReadOnlyCollection<(Guid ProductoId, decimal Cantidad)> items,
        CancellationToken cancellationToken = default);
}
