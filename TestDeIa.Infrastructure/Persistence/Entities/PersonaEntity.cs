namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class PersonaEntity
{
    public Guid Id { get; set; }

    public string TipoIdentificacion { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public DateOnly? FechaNacimiento { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystemRecord { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ClienteEntity> Clientes { get; set; } = [];

    public ICollection<SecurityUserEntity> SecurityUsers { get; set; } = [];
}
