using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Application.Modules.Reporteria.Ports.In;

public interface IReporteVentasUseCase
{
    Task<ReporteVentasResponse> GetVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}
