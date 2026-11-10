namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EmpresaSmtpConfiguracionEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; } = 587;
    public bool UsarSsl { get; set; } = true;
    public bool RequiereAutenticacion { get; set; } = true;
    public string? Usuario { get; set; }
    public string? ContrasenaProtegida { get; set; }
    public string CorreoRemitente { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
