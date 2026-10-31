namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class Bodega
{
    public Bodega(
        Guid id,
        Guid empresaId,
        string codigo,
        string nombre,
        string? direccion,
        bool esPrincipal,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        EmpresaId = empresaId;
        Codigo = codigo;
        Nombre = nombre;
        Direccion = direccion;
        EsPrincipal = esPrincipal;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public Guid EmpresaId { get; }

    public string Codigo { get; }

    public string Nombre { get; }

    public string? Direccion { get; }

    public bool EsPrincipal { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
