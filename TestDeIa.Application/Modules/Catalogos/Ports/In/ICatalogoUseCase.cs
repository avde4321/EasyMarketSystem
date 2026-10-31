using TestDeIa.Shared.Requests.Catalogos;
using TestDeIa.Shared.Responses.Catalogos;

namespace TestDeIa.Application.Modules.Catalogos.Ports.In;

public interface ICatalogoUseCase
{
    Task<IReadOnlyCollection<CatalogoResponse>> GetCatalogosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CatalogoItemResponse>> GetItemsAsync(string catalogoCodigo, bool onlyActive = false, CancellationToken cancellationToken = default);

    Task<CatalogoItemResponse> CreateItemAsync(CatalogoItemRequest request, CancellationToken cancellationToken = default);

    Task<CatalogoItemResponse?> UpdateItemAsync(Guid id, CatalogoItemRequest request, CancellationToken cancellationToken = default);
}
