using TestDeIa.Application.Modules.Compras.Models;
using TestDeIa.Domain.Modules.Compras.Entities;

namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface IEstudioMercadoAnaliticoService
{
    Task<EstudioMercadoAnalisisResult> GenerarAsync(
        IReadOnlyCollection<EstudioMercadoTopProducto> topProductos,
        IReadOnlyCollection<ProductoConsumoHistorico> historial,
        IReadOnlyCollection<EstudioMercadoCompra> estudiosHistoricos,
        int diasMesProximo,
        CancellationToken cancellationToken = default);
}
