using TestDeIa.Shared.Responses.Financiero;

namespace TestDeIa.Application.Modules.Financiero.Ports.In;

public interface IFinancieroReportesUseCase
{
    Task<ConsolidadoIvaMensualResponse> ObtenerConsolidadoIvaAsync(
        int mes,
        int anio,
        string? puntoEmision = null,
        string? cajero = null,
        CancellationToken cancellationToken = default);
}
