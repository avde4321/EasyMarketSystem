namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ClienteEntity
{
    public Guid Id { get; set; }

    public Guid PersonaId { get; set; }

    public PersonaEntity Persona { get; set; } = default!;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
