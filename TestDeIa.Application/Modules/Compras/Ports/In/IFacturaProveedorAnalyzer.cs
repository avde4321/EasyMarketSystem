using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface IFacturaProveedorAnalyzer
{
    Task<FacturaProveedorAnalisisResponse> AnalyzeAsync(
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default);
}
