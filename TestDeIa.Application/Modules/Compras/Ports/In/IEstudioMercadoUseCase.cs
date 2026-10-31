using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface IEstudioMercadoUseCase
{
    Task<EstudioMercadoCompraResponse> GenerarAsync(int mes, int anio, CancellationToken cancellationToken = default);
}
