using TestDeIa.Shared.Personas;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class PersonaNaturalEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string PrimerNombre { get; set; } = string.Empty;

    public string? SegundoNombre { get; set; }

    public string PrimerApellido { get; set; } = string.Empty;

    public string? SegundoApellido { get; set; }

    public TipoDocumentoPersonaNatural TipoDocumento { get; set; }

    public string NumeroDocumento { get; set; } = string.Empty;

    public bool TieneRuc { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string Direccion { get; set; } = string.Empty;
}
