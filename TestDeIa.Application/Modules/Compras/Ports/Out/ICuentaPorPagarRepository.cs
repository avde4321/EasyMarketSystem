using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface ICuentaPorPagarRepository
{
    Task<CuentaPorPagar> CreateAsync(CuentaPorPagar cuentaPorPagar, CancellationToken cancellationToken = default);
    Task<PagedResultResponse<CuentaPorPagar>> GetPagedAsync(string? term, Guid? proveedorId, int skip, int take, CancellationToken cancellationToken = default);
    Task<CuentasPorPagarResumen> GetResumenAsync(CancellationToken cancellationToken = default);
    Task<CuentaPorPagar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CuentaPorPagar> RegistrarAbonoAsync(PagoCxP pago, Guid usuarioId, CancellationToken cancellationToken = default);
}
