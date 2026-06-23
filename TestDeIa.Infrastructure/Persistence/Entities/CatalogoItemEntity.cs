namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CatalogoItemEntity
{
    public Guid Id { get; set; }

    public Guid CatalogoId { get; set; }

    public CatalogoEntity Catalogo { get; set; } = default!;

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int Orden { get; set; }

    public bool IsActive { get; set; }
}
