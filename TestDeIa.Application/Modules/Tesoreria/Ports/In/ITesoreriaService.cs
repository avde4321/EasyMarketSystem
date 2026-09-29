using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Application.Modules.Tesoreria.Ports.In;

public interface ITesoreriaService
{
    Task<IReadOnlyCollection<CuentaBancariaDto>> GetCuentasBancariasAsync(CancellationToken cancellationToken = default);

    Task<CuentaBancariaDto> SaveCuentaBancariaAsync(Guid? id, CuentaBancariaRequest request, CancellationToken cancellationToken = default);

    Task<ExtractoBancarioHeaderDto> ImportarExtractoAsync(
        Guid cuentaBancariaId,
        Stream fileStream,
        string fileName,
        string? formato,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MovimientoTesoreriaDto>> GetMovimientosPendientesAsync(
        Guid cuentaBancariaId,
        DateTime? desde,
        DateTime? hasta,
        string? term,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExtractoBancarioDetalleDto>> GetExtractosPendientesAsync(
        Guid cuentaBancariaId,
        DateTime? desde,
        DateTime? hasta,
        byte? tipoMovimiento,
        string? term,
        CancellationToken cancellationToken = default);
}
