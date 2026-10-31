namespace TestDeIa.Domain.Modules.Personas.Entities;

public sealed class PersonaNatural
{
    public Guid Id { get; init; }

    public Guid EmpresaId { get; init; }

    public string PrimerNombre { get; init; } = string.Empty;

    public string? SegundoNombre { get; init; }

    public string PrimerApellido { get; init; } = string.Empty;

    public string? SegundoApellido { get; init; }

    public string TipoDocumento { get; init; } = string.Empty;

    public string NumeroDocumento { get; init; } = string.Empty;

    public bool TieneRuc { get; init; }

    public string? Email { get; init; }

    public string? Telefono { get; init; }

    public string Direccion { get; init; } = string.Empty;
}
