namespace TestDeIa.Domain.Modules.Empresa.Entities;

public sealed class EmpresaEmisora
{
    public EmpresaEmisora(
        Guid id,
        Guid ownerUserId,
        string razonSocial,
        string? nombreComercial,
        string ruc,
        string direccionMatriz,
        string? direccionEstablecimiento,
        string establecimiento,
        string puntoEmision,
        string ambienteSri,
        bool modoDesarrollo,
        string tipoEmision,
        bool obligadoContabilidad,
        string? contribuyenteEspecial,
        string? regimenRimpe,
        string? agenteRetencionResolucion,
        string? certificadoNombreArchivo,
        byte[]? certificadoContenido,
        string? certificadoClave,
        IReadOnlyCollection<EmpresaPuntoEmision> puntosEmision,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        RazonSocial = razonSocial;
        NombreComercial = nombreComercial;
        Ruc = ruc;
        DireccionMatriz = direccionMatriz;
        DireccionEstablecimiento = direccionEstablecimiento;
        Establecimiento = establecimiento;
        PuntoEmision = puntoEmision;
        AmbienteSri = ambienteSri;
        ModoDesarrollo = modoDesarrollo;
        TipoEmision = tipoEmision;
        ObligadoContabilidad = obligadoContabilidad;
        ContribuyenteEspecial = contribuyenteEspecial;
        RegimenRimpe = regimenRimpe;
        AgenteRetencionResolucion = agenteRetencionResolucion;
        CertificadoNombreArchivo = certificadoNombreArchivo;
        CertificadoContenido = certificadoContenido;
        CertificadoClave = certificadoClave;
        PuntosEmision = puntosEmision;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }
    public Guid OwnerUserId { get; }
    public string RazonSocial { get; }
    public string? NombreComercial { get; }
    public string Ruc { get; }
    public string DireccionMatriz { get; }
    public string? DireccionEstablecimiento { get; }
    public string Establecimiento { get; }
    public string PuntoEmision { get; }
    public string AmbienteSri { get; }
    public bool ModoDesarrollo { get; }
    public string TipoEmision { get; }
    public bool ObligadoContabilidad { get; }
    public string? ContribuyenteEspecial { get; }
    public string? RegimenRimpe { get; }
    public string? AgenteRetencionResolucion { get; }
    public string? CertificadoNombreArchivo { get; }
    public byte[]? CertificadoContenido { get; }
    public string? CertificadoClave { get; }
    public IReadOnlyCollection<EmpresaPuntoEmision> PuntosEmision { get; }
    public bool IsActive { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; }
}
