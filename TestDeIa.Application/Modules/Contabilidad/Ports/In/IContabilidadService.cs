namespace TestDeIa.Application.Modules.Contabilidad.Ports.In;

public interface IContabilidadService
{
    Task<string> GenerarAsientoDesdeOrigenAsync(Guid transaccionId, string moduloOrigen, CancellationToken cancellationToken = default);
}
