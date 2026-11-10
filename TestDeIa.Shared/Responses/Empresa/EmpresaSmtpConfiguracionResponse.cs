namespace TestDeIa.Shared.Responses.Empresa;

public sealed class EmpresaSmtpConfiguracionResponse
{
    public Guid EmpresaId { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public bool UsarSsl { get; set; }
    public bool RequiereAutenticacion { get; set; }
    public string? Usuario { get; set; }
    public bool ContrasenaConfigurada { get; set; }
    public string CorreoRemitente { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
    public DateTimeOffset? Actualizado { get; set; }
}
