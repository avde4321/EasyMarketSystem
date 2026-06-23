using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Domain.Modules.Catalogos.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Catalogos;

public sealed class EfCatalogoRepository : ICatalogoRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfCatalogoRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Catalogo>> GetCatalogosAsync(CancellationToken cancellationToken = default)
    {
        var catalogos = await dbContext.Catalogos
            .AsNoTracking()
            .Include(catalogo => catalogo.Items.OrderBy(item => item.Orden).ThenBy(item => item.Nombre))
            .OrderBy(catalogo => catalogo.Nombre)
            .ToListAsync(cancellationToken);

        return catalogos.Select(MapCatalogo).ToArray();
    }

    public async Task<IReadOnlyCollection<CatalogoItem>> GetItemsAsync(string catalogoCodigo, bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        var items = await dbContext.CatalogoItems
            .AsNoTracking()
            .Include(item => item.Catalogo)
            .Where(item => item.Catalogo.Codigo == catalogoCodigo && (!onlyActive || item.IsActive))
            .OrderBy(item => item.Orden)
            .ThenBy(item => item.Nombre)
            .ToListAsync(cancellationToken);

        return items.Select(MapItem).ToArray();
    }

    public Task<bool> ExistsActiveItemAsync(string catalogoCodigo, string itemCodigo, CancellationToken cancellationToken = default)
    {
        return dbContext.CatalogoItems
            .Include(item => item.Catalogo)
            .AnyAsync(item =>
                item.Catalogo.Codigo == catalogoCodigo &&
                item.Codigo == itemCodigo &&
                item.IsActive,
                cancellationToken);
    }

    public async Task<CatalogoItem?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.CatalogoItems
            .AsNoTracking()
            .Include(current => current.Catalogo)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return item is null ? null : MapItem(item);
    }

    public Task<bool> ExistsItemCodeAsync(string catalogoCodigo, string itemCodigo, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CatalogoItems
            .Include(item => item.Catalogo)
            .AnyAsync(item =>
                item.Catalogo.Codigo == catalogoCodigo &&
                item.Codigo == itemCodigo &&
                (!excludedId.HasValue || item.Id != excludedId.Value),
                cancellationToken);
    }

    public async Task<CatalogoItem> CreateItemAsync(CatalogoItem item, CancellationToken cancellationToken = default)
    {
        var catalogo = await dbContext.Catalogos.FirstOrDefaultAsync(current => current.Codigo == item.CatalogoCodigo, cancellationToken)
            ?? throw new InvalidOperationException("No existe el catalogo indicado.");

        var entity = new CatalogoItemEntity
        {
            Id = item.Id,
            CatalogoId = catalogo.Id,
            Codigo = item.Codigo,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            Orden = item.Orden,
            IsActive = item.IsActive
        };

        dbContext.CatalogoItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        entity.Catalogo = catalogo;
        return MapItem(entity);
    }

    public async Task<CatalogoItem?> UpdateItemAsync(CatalogoItem item, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.CatalogoItems
            .Include(current => current.Catalogo)
            .FirstOrDefaultAsync(current => current.Id == item.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (!string.Equals(entity.Catalogo.Codigo, item.CatalogoCodigo, StringComparison.Ordinal))
        {
            var newCatalogo = await dbContext.Catalogos.FirstOrDefaultAsync(current => current.Codigo == item.CatalogoCodigo, cancellationToken)
                ?? throw new InvalidOperationException("No existe el catalogo indicado.");

            entity.CatalogoId = newCatalogo.Id;
            entity.Catalogo = newCatalogo;
        }

        entity.Codigo = item.Codigo;
        entity.Nombre = item.Nombre;
        entity.Descripcion = item.Descripcion;
        entity.Orden = item.Orden;
        entity.IsActive = item.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapItem(entity);
    }

    private static Catalogo MapCatalogo(CatalogoEntity entity)
    {
        return new Catalogo(
            entity.Id,
            entity.Codigo,
            entity.Nombre,
            entity.Descripcion,
            entity.IsActive,
            entity.Items.OrderBy(item => item.Orden).ThenBy(item => item.Nombre).Select(MapItem).ToArray());
    }

    private static CatalogoItem MapItem(CatalogoItemEntity entity)
    {
        return new CatalogoItem(
            entity.Id,
            entity.CatalogoId,
            entity.Catalogo.Codigo,
            entity.Codigo,
            entity.Nombre,
            entity.Descripcion,
            entity.Orden,
            entity.IsActive);
    }
}
