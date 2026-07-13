namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ProveedorEntity
{
    public Guid PersonaId { get; set; }
    public Guid EmpresaId { get; set; }
    public PersonaEntity Persona { get; set; } = default!;
    public string CodigoRetencionIvaDefault { get; set; } = string.Empty;
    public string CodigoRetencionRentaDefault { get; set; } = string.Empty;
    public bool PermiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public string EstadoProveedor { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
}
