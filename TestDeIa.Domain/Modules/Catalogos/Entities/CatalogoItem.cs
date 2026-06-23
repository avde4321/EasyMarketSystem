namespace TestDeIa.Domain.Modules.Catalogos.Entities;

public sealed class CatalogoItem
{
    public CatalogoItem(
        Guid id,
        Guid catalogoId,
        string catalogoCodigo,
        string codigo,
        string nombre,
        string? descripcion,
        int orden,
        bool isActive)
    {
        Id = id;
        CatalogoId = catalogoId;
        CatalogoCodigo = catalogoCodigo;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Orden = orden;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public Guid CatalogoId { get; }

    public string CatalogoCodigo { get; }

    public string Codigo { get; }

    public string Nombre { get; }

    public string? Descripcion { get; }

    public int Orden { get; }

    public bool IsActive { get; }
}
