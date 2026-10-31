namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityAuditLog
{
    public SecurityAuditLog(
        Guid id,
        Guid empresaId,
        Guid usuarioId,
        DateTimeOffset fechaEvento,
        SecurityAuditEventType tipoEvento,
        string? direccionIp,
        string? detalles)
    {
        Id = id;
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        FechaEvento = fechaEvento;
        TipoEvento = tipoEvento;
        DireccionIp = direccionIp;
        Detalles = detalles;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public Guid UsuarioId { get; }
    public DateTimeOffset FechaEvento { get; }
    public SecurityAuditEventType TipoEvento { get; }
    public string? DireccionIp { get; }
    public string? Detalles { get; }
}
