using TestDeIa.Shared.Responses.Reporteria;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Reporteria.Ports.In;

public interface IReporteService
{
    Task<PagedResultResponse<KardexValorizadoReporteItemResponse>> ObtenerKardexValorizadoAsync(DateTime desde, DateTime hasta, Guid? bodegaId, int skip, int take, CancellationToken cancellationToken = default);
    Task<PagedResultResponse<CierreCajaReporteItemResponse>> ObtenerCierresCajaAsync(DateTime desde, DateTime hasta, int skip, int take, CancellationToken cancellationToken = default);
}
