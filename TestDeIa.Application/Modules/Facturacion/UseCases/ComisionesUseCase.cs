using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.UseCases;

public sealed class ComisionesUseCase : IComisionesUseCase
{
    private readonly IComisionesRepository comisionesRepository;

    public ComisionesUseCase(IComisionesRepository comisionesRepository)
    {
        this.comisionesRepository = comisionesRepository;
    }

    public Task<LiquidacionComisionResponse> GetLiquidacionAsync(DateOnly desde, DateOnly hasta, Guid? operadorId = null, CancellationToken cancellationToken = default)
    {
        if (hasta < desde)
        {
            throw new InvalidOperationException("La fecha hasta no puede ser menor que la fecha desde.");
        }

        return comisionesRepository.GetLiquidacionAsync(desde, hasta, operadorId, cancellationToken);
    }
}