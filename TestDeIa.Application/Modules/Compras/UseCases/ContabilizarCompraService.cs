using TestDeIa.Application.Modules.Contabilidad.Ports.In;

namespace TestDeIa.Application.Modules.Compras.UseCases;

public sealed class ContabilizarCompraService(IContabilidadService contabilidadService)
{
    public Task<string> ContabilizarAsync(Guid compraId, CancellationToken cancellationToken = default)
    {
        return contabilidadService.GenerarAsientoDesdeOrigenAsync(compraId, "Compras", cancellationToken);
    }
}
