namespace TestDeIa.Domain.Modules.Catalogos.Entities;

public sealed class Catalogo
{
    public Catalogo(
        Guid id,
        string codigo,
        string nombre,
        string? descripcion,
        bool isActive,
        IReadOnlyCollection<CatalogoItem> items)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        IsActive = isActive;
        Items = items;
    }

    public Guid Id { get; }

    public string Codigo { get; }

    public string Nombre { get; }

    public string? Descripcion { get; }

    public bool IsActive { get; }

    public IReadOnlyCollection<CatalogoItem> Items { get; }
}
