namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class NumeracionHistorialEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Serie { get; set; } = string.Empty;
    public string AmbienteSri { get; set; } = string.Empty;
    public string CodigoDocumento { get; set; } = string.Empty;
    public long Anterior { get; set; }
    public long Nuevo { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
