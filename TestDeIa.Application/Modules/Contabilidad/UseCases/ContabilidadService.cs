using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class ContabilidadService(IContabilidadRepository contabilidadRepository) : IContabilidadService
{
    public Task<string> GenerarAsientoDesdeOrigenAsync(Guid transaccionId, string moduloOrigen, CancellationToken cancellationToken = default)
    {
        if (transaccionId == Guid.Empty)
        {
            throw new InvalidOperationException("La transacción de origen no es válida para generar el asiento contable.");
        }

        if (string.IsNullOrWhiteSpace(moduloOrigen))
        {
            throw new InvalidOperationException("El módulo de origen es obligatorio para generar el asiento contable.");
        }

        return contabilidadRepository.GenerarAsientoDesdeOrigenAsync(transaccionId, moduloOrigen, cancellationToken);
    }
}
