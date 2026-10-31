using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.Ports.In;

public interface IContabilidadService
{
    Task<string> GenerarAsientoDesdeOrigenAsync(Guid transaccionId, string moduloOrigen, CancellationToken cancellationToken = default);
    Task<AjusteInventarioContableResponse> AjustarInventarioContableAsync(bool generarAsiento, CancellationToken cancellationToken = default);
}
