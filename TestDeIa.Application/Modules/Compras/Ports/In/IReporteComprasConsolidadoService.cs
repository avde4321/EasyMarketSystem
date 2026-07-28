using TestDeIa.Shared.Compras;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface IReporteComprasConsolidadoService
{
    Task<ReporteComprasConsolidadoResponse> ConsultarAsync(
        DateTime fechaInicio,
        DateTime fechaFin,
        NaturalezaCompra? naturalezaCompra,
        CancellationToken cancellationToken = default);
}
