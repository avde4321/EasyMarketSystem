namespace TestDeIa.Shared.Responses.Security;

public sealed class SecurityAuditLogResponse
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTimeOffset FechaEvento { get; set; }

    public string TipoEvento { get; set; } = string.Empty;

    public string? DireccionIp { get; set; }

    public string? Detalles { get; set; }
}
