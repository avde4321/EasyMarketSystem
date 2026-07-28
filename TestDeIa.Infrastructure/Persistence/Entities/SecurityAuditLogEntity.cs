using TestDeIa.Domain.Modules.Security.Entities;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityAuditLogEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTimeOffset FechaEvento { get; set; }
    public SecurityAuditEventType TipoEvento { get; set; }
    public string? DireccionIP { get; set; }
    public string? Detalles { get; set; }
}
