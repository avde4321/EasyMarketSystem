namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CertificadoDigitalEmpresaEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;

    public string Nombre { get; set; } = string.Empty;

    public string NombreArchivo { get; set; } = string.Empty;

    public byte[] Contenido { get; set; } = [];

    public string Clave { get; set; } = string.Empty;

    public string? Sujeto { get; set; }

    public string? Emisor { get; set; }

    public string? NumeroSerie { get; set; }

    public string? HuellaDigital { get; set; }

    public DateTimeOffset FechaInicioVigencia { get; set; }

    public DateTimeOffset FechaFinVigencia { get; set; }

    public bool IsActive { get; set; }

    public bool EsPrincipal { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UsuarioCreacionId { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UsuarioModificacionId { get; set; }
}
