namespace TestDeIa.Domain.Modules.Clientes.Entities;

public sealed class Cliente
{
    public Cliente(
        Guid id,
        Guid personaId,
        string tipoIdentificacion,
        string identificacion,
        string nombres,
        string apellidos,
        string? email,
        string? telefono,
        string? direccion,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        PersonaId = personaId;
        TipoIdentificacion = tipoIdentificacion;
        Identificacion = identificacion;
        Nombres = nombres;
        Apellidos = apellidos;
        Email = email;
        Telefono = telefono;
        Direccion = direccion;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public Guid PersonaId { get; }

    public string TipoIdentificacion { get; }

    public string Identificacion { get; }

    public string Nombres { get; }

    public string Apellidos { get; }

    public string? Email { get; }

    public string? Telefono { get; }

    public string? Direccion { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
