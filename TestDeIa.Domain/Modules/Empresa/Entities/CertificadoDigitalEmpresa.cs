namespace TestDeIa.Domain.Modules.Empresa.Entities;

public sealed class CertificadoDigitalEmpresa
{
    public CertificadoDigitalEmpresa(
        Guid id,
        Guid empresaId,
        string empresaNombre,
        string empresaRuc,
        string nombre,
        string nombreArchivo,
        byte[] contenido,
        string clave,
        string? sujeto,
        string? emisor,
        string? numeroSerie,
        string? huellaDigital,
        DateTimeOffset fechaInicioVigencia,
        DateTimeOffset fechaFinVigencia,
        bool isActive,
        bool esPrincipal,
        DateTimeOffset createdAt,
        Guid? usuarioCreacionId,
        DateTimeOffset? updatedAt,
        Guid? usuarioModificacionId)
    {
        Id = id;
        EmpresaId = empresaId;
        EmpresaNombre = empresaNombre;
        EmpresaRuc = empresaRuc;
        Nombre = nombre;
        NombreArchivo = nombreArchivo;
        Contenido = contenido;
        Clave = clave;
        Sujeto = sujeto;
        Emisor = emisor;
        NumeroSerie = numeroSerie;
        HuellaDigital = huellaDigital;
        FechaInicioVigencia = fechaInicioVigencia;
        FechaFinVigencia = fechaFinVigencia;
        IsActive = isActive;
        EsPrincipal = esPrincipal;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
        UpdatedAt = updatedAt;
        UsuarioModificacionId = usuarioModificacionId;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public string EmpresaNombre { get; }
    public string EmpresaRuc { get; }
    public string Nombre { get; }
    public string NombreArchivo { get; }
    public byte[] Contenido { get; }
    public string Clave { get; }
    public string? Sujeto { get; }
    public string? Emisor { get; }
    public string? NumeroSerie { get; }
    public string? HuellaDigital { get; }
    public DateTimeOffset FechaInicioVigencia { get; }
    public DateTimeOffset FechaFinVigencia { get; }
    public bool IsActive { get; }
    public bool EsPrincipal { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid? UsuarioCreacionId { get; }
    public DateTimeOffset? UpdatedAt { get; }
    public Guid? UsuarioModificacionId { get; }
}
