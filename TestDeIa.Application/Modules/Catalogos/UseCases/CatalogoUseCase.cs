using TestDeIa.Application.Modules.Catalogos.Ports.In;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Domain.Modules.Catalogos.Entities;
using TestDeIa.Shared.Requests.Catalogos;
using TestDeIa.Shared.Responses.Catalogos;

namespace TestDeIa.Application.Modules.Catalogos.UseCases;

public sealed class CatalogoUseCase : ICatalogoUseCase
{
    private readonly ICatalogoRepository catalogoRepository;

    public CatalogoUseCase(ICatalogoRepository catalogoRepository)
    {
        this.catalogoRepository = catalogoRepository;
    }

    public async Task<IReadOnlyCollection<CatalogoResponse>> GetCatalogosAsync(CancellationToken cancellationToken = default)
    {
        var catalogos = await catalogoRepository.GetCatalogosAsync(cancellationToken);
        return catalogos.Select(MapCatalogo).ToArray();
    }

    public async Task<IReadOnlyCollection<CatalogoItemResponse>> GetItemsAsync(string catalogoCodigo, bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(catalogoCodigo))
        {
            throw new InvalidOperationException("El codigo del catalogo es obligatorio.");
        }

        var items = await catalogoRepository.GetItemsAsync(catalogoCodigo.Trim(), onlyActive, cancellationToken);
        return items.Select(MapItem).ToArray();
    }

    public async Task<CatalogoItemResponse> CreateItemAsync(CatalogoItemRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        if (await catalogoRepository.ExistsItemCodeAsync(request.CatalogoCodigo.Trim(), request.Codigo.Trim(), cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un item con ese codigo dentro del catalogo.");
        }

        var item = new CatalogoItem(
            Guid.NewGuid(),
            Guid.Empty,
            request.CatalogoCodigo.Trim(),
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Descripcion),
            request.Orden,
            request.IsActive);

        return MapItem(await catalogoRepository.CreateItemAsync(item, cancellationToken));
    }

    public async Task<CatalogoItemResponse?> UpdateItemAsync(Guid id, CatalogoItemRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var current = await catalogoRepository.GetItemByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        if (await catalogoRepository.ExistsItemCodeAsync(request.CatalogoCodigo.Trim(), request.Codigo.Trim(), id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro item con ese codigo dentro del catalogo.");
        }

        var item = new CatalogoItem(
            id,
            current.CatalogoId,
            request.CatalogoCodigo.Trim(),
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Descripcion),
            request.Orden,
            request.IsActive);

        var updated = await catalogoRepository.UpdateItemAsync(item, cancellationToken);
        return updated is null ? null : MapItem(updated);
    }

    private static CatalogoResponse MapCatalogo(Catalogo catalogo)
    {
        return new CatalogoResponse
        {
            Id = catalogo.Id,
            Codigo = catalogo.Codigo,
            Nombre = catalogo.Nombre,
            Descripcion = catalogo.Descripcion,
            IsActive = catalogo.IsActive,
            Items = catalogo.Items.Select(MapItem).ToArray()
        };
    }

    private static CatalogoItemResponse MapItem(CatalogoItem item)
    {
        return new CatalogoItemResponse
        {
            Id = item.Id,
            CatalogoId = item.CatalogoId,
            CatalogoCodigo = item.CatalogoCodigo,
            Codigo = item.Codigo,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            Orden = item.Orden,
            IsActive = item.IsActive
        };
    }

    private static void ValidateRequest(CatalogoItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CatalogoCodigo))
        {
            throw new InvalidOperationException("El catalogo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            throw new InvalidOperationException("El codigo del item es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new InvalidOperationException("El nombre del item es obligatorio.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
