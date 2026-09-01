namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class DocumentoAdjuntoEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmpresaId { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;

    public string Modulo { get; set; } = string.Empty;

    public string EntidadTipo { get; set; } = string.Empty;

    public Guid EntidadId { get; set; }

    public string TipoAdjunto { get; set; } = string.Empty;

    public string NombreArchivo { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string RutaStorage { get; set; } = string.Empty;

    public string HashSHA256 { get; set; } = string.Empty;

    public long TamanoBytes { get; set; }

    public Guid? CreadoPorUsuarioId { get; set; }

    public string Origen { get; set; } = "Automatico";

    public bool EsActivo { get; set; } = true;

    public int Version { get; set; } = 1;

    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}
