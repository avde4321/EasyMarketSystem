namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CatalogoEntity
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool IsActive { get; set; }

    public ICollection<CatalogoItemEntity> Items { get; set; } = [];
}
