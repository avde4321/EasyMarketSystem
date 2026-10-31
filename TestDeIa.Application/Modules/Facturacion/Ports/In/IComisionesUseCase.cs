using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.Ports.In;

public interface IComisionesUseCase
{
    Task<LiquidacionComisionResponse> GetLiquidacionAsync(DateOnly desde, DateOnly hasta, Guid? operadorId = null, CancellationToken cancellationToken = default);
}