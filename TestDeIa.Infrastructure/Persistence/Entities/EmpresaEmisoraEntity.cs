namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EmpresaEmisoraEntity
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string DireccionMatriz { get; set; } = string.Empty;
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
    public byte[]? CertificadoContenido { get; set; }
    public string? CertificadoClave { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public ICollection<SecurityUserEmpresaEntity> UserAssignments { get; set; } = [];
}
