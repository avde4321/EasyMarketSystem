using TestDeIa.Domain.Modules.Financiero.Entities;

namespace TestDeIa.Application.Modules.Financiero.Services;

public interface IAnalizadorFiscalIAService
{
    AnalisisFiscalIAResult Analizar(
        ConsolidadoIvaMensual consolidadoActual,
        IReadOnlyCollection<MemoriaAnalisisFiscal> memoriasHistoricas);
}
