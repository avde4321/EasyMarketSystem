using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.Ports.Out;

public interface IComisionesRepository
{
    Task<LiquidacionComisionResponse> GetLiquidacionAsync(DateOnly desde, DateOnly hasta, Guid? operadorId = null, CancellationToken cancellationToken = default);
}