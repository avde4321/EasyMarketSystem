using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class ContabilidadService(IContabilidadRepository contabilidadRepository) : IContabilidadService
{
    public Task<string> GenerarAsientoDesdeOrigenAsync(Guid transaccionId, string moduloOrigen, CancellationToken cancellationToken = default)
    {
        if (transaccionId == Guid.Empty)
        {
            throw new InvalidOperationException("La transaccion de origen no es valida para generar el asiento contable.");
        }

        if (string.IsNullOrWhiteSpace(moduloOrigen))
        {
            throw new InvalidOperationException("El modulo de origen es obligatorio para generar el asiento contable.");
        }

        return contabilidadRepository.GenerarAsientoDesdeOrigenAsync(transaccionId, moduloOrigen, cancellationToken);
    }

    public Task<AjusteInventarioContableResponse> AjustarInventarioContableAsync(bool generarAsiento, CancellationToken cancellationToken = default) =>
        contabilidadRepository.AjustarInventarioContableAsync(generarAsiento, cancellationToken);
}
