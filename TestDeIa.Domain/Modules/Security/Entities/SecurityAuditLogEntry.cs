namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityAuditLogEntry
{
    public SecurityAuditLogEntry(
        Guid id,
        Guid empresaId,
        Guid usuarioId,
        string userName,
        string displayName,
        DateTimeOffset fechaEvento,
        SecurityAuditEventType tipoEvento,
        string? direccionIp,
        string? detalles)
    {
        Id = id;
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        UserName = userName;
        DisplayName = displayName;
        FechaEvento = fechaEvento;
        TipoEvento = tipoEvento;
        DireccionIp = direccionIp;
        Detalles = detalles;
    }

    public Guid Id { get; }

    public Guid EmpresaId { get; }

    public Guid UsuarioId { get; }

    public string UserName { get; }

    public string DisplayName { get; }

    public DateTimeOffset FechaEvento { get; }

    public SecurityAuditEventType TipoEvento { get; }

    public string? DireccionIp { get; }

    public string? Detalles { get; }
}
