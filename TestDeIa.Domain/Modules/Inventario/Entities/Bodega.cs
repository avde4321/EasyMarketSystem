namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class Bodega
{
    public Bodega(
        Guid id,
        Guid empresaId,
        string nombre,
        string? direccion,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        EmpresaId = empresaId;
        Nombre = nombre;
        Direccion = direccion;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public Guid EmpresaId { get; }

    public string Nombre { get; }

    public string? Direccion { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
