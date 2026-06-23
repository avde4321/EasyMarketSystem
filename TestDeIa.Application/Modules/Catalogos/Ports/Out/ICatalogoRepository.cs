using TestDeIa.Domain.Modules.Catalogos.Entities;

namespace TestDeIa.Application.Modules.Catalogos.Ports.Out;

public interface ICatalogoRepository
{
    Task<IReadOnlyCollection<Catalogo>> GetCatalogosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CatalogoItem>> GetItemsAsync(string catalogoCodigo, bool onlyActive = false, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveItemAsync(string catalogoCodigo, string itemCodigo, CancellationToken cancellationToken = default);

    Task<CatalogoItem?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CatalogoItem> CreateItemAsync(CatalogoItem item, CancellationToken cancellationToken = default);

    Task<CatalogoItem?> UpdateItemAsync(CatalogoItem item, CancellationToken cancellationToken = default);

    Task<bool> ExistsItemCodeAsync(string catalogoCodigo, string itemCodigo, Guid? excludedId = null, CancellationToken cancellationToken = default);
}
