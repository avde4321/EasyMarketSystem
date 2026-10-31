using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Application.Modules.Reporteria.Ports.Out;

public interface IReporteVentasRepository
{
    Task<ReporteVentasResponse> GetVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}
