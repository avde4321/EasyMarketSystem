namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ClienteEntity
{
    public Guid PersonaId { get; set; }

    public Guid EmpresaId { get; set; }

    public PersonaEntity Persona { get; set; } = default!;

    public string? CorreoFacturacionElectronica { get; set; }

    public string TipoCliente { get; set; } = string.Empty;

    public bool ObligadoContabilidad { get; set; }

    public bool EsContribuyenteEspecial { get; set; }

    public bool PermiteCredito { get; set; }

    public decimal LimiteCredito { get; set; }

    public int DiasCreditoMaximo { get; set; }

    public string EstadoCredito { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid UsuarioCreacionId { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UsuarioModificacionId { get; set; }
}
