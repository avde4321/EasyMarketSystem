using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Application.Modules.Reporteria.Ports.Out;
using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Application.Modules.Reporteria.UseCases;

public sealed class ReporteVentasUseCase(IReporteVentasRepository reporteVentasRepository) : IReporteVentasUseCase
{
    public Task<ReporteVentasResponse> GetVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
    {
        if (fechaFin.Date < fechaInicio.Date)
        {
            throw new InvalidOperationException("La fecha final no puede ser menor a la fecha inicial.");
        }

        return reporteVentasRepository.GetVentasAsync(fechaInicio.Date, fechaFin.Date, cancellationToken);
    }
}
