using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Application.Modules.Empresa.Ports.In;

public interface INumeracionService
{
    Task<IReadOnlyCollection<NumeracionResponse>> ConsultarAsync(string ambiente, CancellationToken ct);
    Task ConfigurarAsync(NumeracionRequest request, CancellationToken ct);
    Task<IReadOnlyCollection<NumeracionHistorialResponse>> HistorialAsync(CancellationToken ct);
}
