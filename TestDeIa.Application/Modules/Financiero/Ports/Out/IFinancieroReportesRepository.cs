using TestDeIa.Domain.Modules.Financiero.Entities;

namespace TestDeIa.Application.Modules.Financiero.Ports.Out;

public interface IFinancieroReportesRepository
{
    Task<ConsolidadoIvaMensual> ObtenerConsolidadoIvaAsync(int mes, int anio, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MemoriaAnalisisFiscal>> ObtenerMemoriasHistoricasAsync(int mes, int anio, int cantidad, CancellationToken cancellationToken = default);
    Task GuardarMemoriaAnalisisFiscalAsync(MemoriaAnalisisFiscal memoria, CancellationToken cancellationToken = default);
}
