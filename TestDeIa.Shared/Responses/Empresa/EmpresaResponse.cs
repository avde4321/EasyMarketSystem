namespace TestDeIa.Shared.Responses.Empresa;

public sealed class EmpresaResponse
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string DireccionMatriz { get; set; } = string.Empty;
    public string? RegionCodigo { get; set; }
    public string? ProvinciaCodigo { get; set; }
    public string? CiudadCodigo { get; set; }
    public string? SectorCodigo { get; set; }
    public string? DireccionEstablecimiento { get; set; }
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public string AmbienteSri { get; set; } = string.Empty;
    public bool ModoDesarrollo { get; set; }
    public string TipoEmision { get; set; } = string.Empty;
    public bool ObligadoContabilidad { get; set; }
    public string? ContribuyenteEspecial { get; set; }
    public string? RegimenRimpe { get; set; }
    public string? AgenteRetencionResolucion { get; set; }
    public string? CertificadoNombreArchivo { get; set; }
    public bool TieneCertificadoConfigurado { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public IReadOnlyCollection<EmpresaPuntoEmisionResponse> PuntosEmision { get; set; } = Array.Empty<EmpresaPuntoEmisionResponse>();
}
