using TestDeIa.Domain.Modules.Compras.Entities;

namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface IEstudioMercadoRepository
{
    Task<IReadOnlyCollection<EstudioMercadoTopProducto>> GetTopProductosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductoConsumoHistorico>> GetHistorialConsumoAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, IReadOnlyCollection<Guid> productoIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<EstudioMercadoCompra>> GetEstudiosHistoricosAsync(int mes, int anio, int take, CancellationToken cancellationToken = default);
    Task SaveAsync(EstudioMercadoCompra estudio, CancellationToken cancellationToken = default);
}
